using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using DataLakeModels.Models.TikTok;
using Serilog.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

using Andromeda.Common;
using Andromeda.Common.Extensions;

namespace Jobs.Fetcher.TikTok.Helpers {
    public static class ApiDataFetcher {
        public static IEnumerable<JObject> GetPosts(string username,
                                                    DateTime lastFetch,
                                                    Logger logger
                                                    ) {
            logger.Information(@"Fetching posts for '" + username + "' after '" + lastFetch + "'.");
            int lastOffset = 0;
            while (true) {
                var somePosts = DatabaseManager.GetPayload(username, lastFetch, lastOffset);
                logger.Information($"Reading next page of results... "
                                   + $"found {somePosts.Count} posts.");

                if (somePosts.Count() == 0) {
                    yield break;
                }
                foreach (var payload in somePosts) {
                    yield return JObject.Parse(payload);
                }
                lastOffset += DatabaseManager._payloadBatchSize;
            }
        }

        public static Video GetTikTokVideoFromJson(JObject videoJson,
                                                   string postId,
                                                   Logger logger) {
            var shareCovers = new List<string>();

            // essential fields, checked for in DoWeHaveTheBareMinimum
            videoJson.TryGetValue("id", out var id);
            videoJson.TryGetValue("duration", out var duration);
            videoJson.TryGetValue("cover", out var cover);
            videoJson.TryGetValue("playAddr", out var playAddr);

            videoJson.TryGetValue("height", out var height);
            videoJson.TryGetValue("width", out var width);
            videoJson.TryGetValue("ratio", out var ratio);
            videoJson.TryGetValue("originCover", out var originCover);
            videoJson.TryGetValue("dynamicCover", out var dynamicCover);
            videoJson.TryGetValue("downloadAddr", out var downloadAddr);
            videoJson.TryGetValue("reflowCover", out var reflowCover);
            videoJson.TryGetValue("bitrate", out var bitrate);
            videoJson.TryGetValue("encodedType", out var encodedType);
            videoJson.TryGetValue("format", out var format);
            videoJson.TryGetValue("videoQuality", out var videoQuality);
            videoJson.TryGetValue("encodeUserTag", out var encodeUserTag);
            videoJson.TryGetValue("codecType", out var codecType);
            videoJson.TryGetValue("definition", out var definition);
            videoJson.TryGetValue("shareCover", out var shareCoverJson);

            foreach (var shareCover in shareCoverJson) {
                shareCovers.Add(shareCover.ToString());
            }

            try {
                var newVideo = new Video() {
                    Id = id.ToString(),
                    Height = height.ToObject<int>(),
                    Width = width.ToObject<int>(),
                    Duration = duration.ToObject<int>(),
                    Ratio = ratio.ToString(),
                    Cover = cover.ToString(),
                    OriginCover = originCover.ToString(),
                    DynamicCover = dynamicCover.ToString(),
                    PlayAddress = playAddr.ToString(),
                    DownloadAddress = downloadAddr.ToString(),
                    ShareCover = shareCovers,
                    ReflowCover = reflowCover.ToString(),
                    BitRate = bitrate.ToObject<int>(),
                    EncodedType = encodedType.ToString(),
                    Format = format.ToString(),
                    VideoQuality = videoQuality.ToString(),
                    EncodedUserTag = encodeUserTag.ToString(),
                    CodecType = codecType.ToString(),
                    Definition = definition.ToString()
                };

                return newVideo;
            } catch {
                logger.Error($"id = {postId} - error converting JToken to object! Returning null");
                return null;
            }
        }

        public static Music GetTikTokMusicFromJson(JToken musicJson) {
            return new Music() {
                       Id = musicJson["id"].ToString(),
                       Title = musicJson["title"].ToString(),
                       PlayUrl = musicJson["playUrl"].ToString(),
                       CoverThumb = musicJson["coverThumb"].ToString(),
                       CoverMedium = musicJson["coverMedium"].ToString(),
                       CoverLarge = musicJson["coverLarge"].ToString(),
                       AuthorName = musicJson["authorName"].ToString(),
                       Original = musicJson["original"].ToObject<bool>(),
                       Duration = musicJson["duration"].ToObject<int>(),
                       Album = musicJson["album"].ToString()
            };
        }

        public static List<Challenge> GetTikTokChallengesFromJson(JToken challengesJson) {
            var newChallenges = new List<Challenge>();
            if (challengesJson == null) {
                return newChallenges;
            }
            foreach (var challenge in challengesJson) {
                newChallenges.Add(new Challenge() {
                    Id = challenge["id"].ToString(),
                    Title = challenge["title"].ToString(),
                    Description = challenge["desc"].ToString(),
                    ProfileThumbnail = challenge["profileThumb"].ToString(),
                    ProfileMedium = challenge["profileMedium"].ToString(),
                    ProfileLarge = challenge["profileLarger"].ToString(),
                    CoverThumbnail = challenge["coverThumb"].ToString(),
                    CoverMedium = challenge["coverMedium"].ToString(),
                    CoverLarge = challenge["coverLarger"].ToString(),
                    IsCommerce = challenge["isCommerce"].ToObject<bool>()
                });
            }
            return newChallenges;
        }

        public static List<Tag> GetTikTokTagsFromJson(JToken tagsJson) {
            var newTags = new List<Tag>();
            if (tagsJson == null) {
                return newTags;
            }
            foreach (var tag in tagsJson) {
                newTags.Add(new Tag() {
                    AweMeId = tag["awemeId"].ToString(),
                    Start = tag["start"].ToObject<int>(),
                    End = tag["end"].ToObject<int>(),
                    HashtagName = tag["hashtagName"].ToString(),
                    HashtagId = tag["hashtagId"].ToString(),
                    Type = tag["type"].ToObject<int>(),
                    UserId = tag["userId"].ToString(),
                    IsCommerce = tag["isCommerce"].ToObject<bool>(),
                    UserUniqueId = tag["userUniqueId"].ToString(),
                    SecureUId = tag["secUid"].ToString(),
                    SubType = tag["subType"].ToObject<int>()
                });
            }
            return newTags;
        }

        public static List<EffectSticker> GetTikTokEffectStickersFromJson(JToken effectStickersJson) {
            var effectStickers = new List<EffectSticker>();
            if (effectStickersJson == null) {
                return effectStickers;
            }
            foreach (var sticker in effectStickersJson) {
                var newEffectSticker = new EffectSticker() {
                    Name = sticker["name"].ToString(),
                    Id = sticker["ID"].ToString()
                };
                effectStickers.Add(newEffectSticker);
            }
            return effectStickers;
        }

        public static Author GetTikTokAuthorFromAuthorJson(JToken authorJson) {
            return new Author() {
                       Id = authorJson["id"].ToString(),
                       UniqueId = authorJson["uniqueId"].ToString(),
                       Nickname = authorJson["nickname"].ToString(),
                       AvatarThumbnail = authorJson["avatarThumb"].ToString(),
                       AvatarMedium = authorJson["avatarMedium"].ToString(),
                       AvatarLarge = authorJson["avatarLarger"].ToString(),
                       Signature = authorJson["signature"].ToString(),
                       Verified = authorJson["verified"].ToObject<bool>(),
                       SecurityUId = authorJson["secUid"].ToString(),
                       Secret = authorJson["secret"].ToObject<bool>(),
                       FTC = authorJson["ftc"].ToObject<bool>(),
                       Relation = authorJson["relation"].ToObject<int>(),
                       OpenFavorite = authorJson["openFavorite"].ToObject<int>(),
                       CommentSetting = authorJson["commentSetting"].ToObject<int>(),
                       DuetSetting = authorJson["duetSetting"].ToObject<int>(),
                       StitchSetting = authorJson["stitchSetting"].ToObject<int>(),
                       PrivateAccount = authorJson["privateAccount"].ToObject<bool>()
            };
        }

        public static Author GetTikTokAuthorFromPostJson(JToken postJson) {
            return new Author() {
                       Id = postJson["authorId"].ToString(),
                       UniqueId = postJson["author"].ToString(),
                       Nickname = postJson["nickname"].ToString()
            };
        }

        public static AuthorStats GetTikTokAuthorStatsFromJson(JObject authorStatsJson, Author author, DateTime postTime) {

            authorStatsJson.TryGetValue("followingCount", out var followingCount);
            authorStatsJson.TryGetValue("followerCount", out var followerCount);
            authorStatsJson.TryGetValue("heartCount", out var heartCount);
            authorStatsJson.TryGetValue("videoCount", out var videoCount);
            authorStatsJson.TryGetValue("diggCount", out var diggCount);
            authorStatsJson.TryGetValue("heart", out var heart);

            return new AuthorStats() {
                       FollowingCount = followingCount?.ToObject<long>() ?? 0,
                       FollowerCount = followerCount?.ToObject<long>() ?? 0,
                       HeartCount = heartCount?.ToObject<long>() ?? 0,
                       VideoCount = videoCount?.ToObject<long>() ?? 0,
                       DiggCount = diggCount?.ToObject<long>() ?? 0,
                       Heart = heart?.ToObject<long>() ?? 0,
                       EventDate = DateTime.Today,
                       ValidityStart = DateTime.UtcNow,
                       ValidityEnd = DateTime.MaxValue,
                       AuthorId = author.Id,
                       Author = author
            };
        }

        public static Post GetTikTokPostFromJson(JToken postJson, Author author, Video video, Music music, List<string> challengeIds, List<string> tagIds, List<string> effectStickerIds) {
            return new Post() {
                       Id = postJson["id"].ToString(),
                       CreateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc).AddSeconds(postJson["createTime"].ToObject<int>()),
                       Description = postJson["desc"].ToString(),
                       DuetInfo = postJson["duetInfo"]["duetFromId"].ToString(),
                       OriginalItem = postJson["originalItem"].ToObject<bool>(),
                       OfficialItem = postJson["officalItem"].ToObject<bool>(),
                       Secret = postJson["secret"].ToObject<bool>(),
                       ForFriend = postJson["forFriend"].ToObject<bool>(),
                       Digged = postJson["digged"].ToObject<bool>(),
                       ItemCommentStatus = postJson["itemCommentStatus"].ToObject<int>(),
                       ShowNotPass = postJson["showNotPass"].ToObject<bool>(),
                       VL1 = postJson["vl1"].ToObject<bool>(),
                       ItemMute = postJson["itemMute"].ToObject<bool>(),
                       Private = postJson["privateItem"].ToObject<bool>(),
                       DuetEnabled = postJson["duetEnabled"].ToObject<bool>(),
                       StitchEnabled = postJson["stitchEnabled"].ToObject<bool>(),
                       ShareEnabled = postJson["shareEnabled"].ToObject<bool>(),
                       IsAd = postJson["isAd"].ToObject<bool>(),
                       DuetDisplay = postJson["duetDisplay"].ToObject<int>(),
                       StitchDisplay = postJson["stitchDisplay"].ToObject<int>(),
                       ValidityStart = DateTime.UtcNow,
                       ValidityEnd = DateTime.MaxValue,
                       AuthorId = author.Id,
                       Author = author,
                       VideoId = video.Id,
                       Video = video,
                       MusicId = music.Id,
                       Music = music,
                       ChallengeIds = challengeIds,
                       TagIds = tagIds,
                       EffectStickerIds = effectStickerIds
            };
        }

        public static PostStats GetTikTokPostStatsJson(JToken postStatsJson, Post post, DateTime postTime) {
            return new PostStats() {
                       DiggCount = postStatsJson["diggCount"].ToObject<long>(),
                       ShareCount = postStatsJson["shareCount"].ToObject<long>(),
                       CommentCount = postStatsJson["commentCount"].ToObject<long>(),
                       PlayCount = postStatsJson["playCount"].ToObject<long>(),
                       EventDate = DateTime.Today,
                       ValidityStart = DateTime.UtcNow,
                       ValidityEnd = DateTime.MaxValue,
                       PostId = post.Id,
                       Post = post
            };
        }

        public static bool TryField(JObject post,
                                    KeyValuePair<string, Type> fieldAndType,
                                    Logger logger
                                    ) {
            JToken tmp;
            string field = fieldAndType.Key;
            Type type = fieldAndType.Value;
            if (!post.TryGetValue(field, out tmp)) {
                logger.Information($"Object rejected - no '{field}' field");
                return false;
            }
            try {
                var extracted = tmp.ToObject(type);
            } catch (Exception exc) {
                logger.Information($"Object rejected - failed to decode '{field}' as '{type}'");
                logger.Information(tmp.ToString());
                logger.Information(exc.ToString());
                return false;
            }

            return true;
        }

        public static bool DoWeHaveTheBareMinimum(JObject post, Logger logger) {
            var topLevelFields = new Dictionary<string, Type>() {
                { "id", typeof(string) },
                { "desc", typeof(string) },

                { "originalItem", typeof(bool) },
                { "officalItem", typeof(bool) },
                { "secret", typeof(bool) },
                { "forFriend", typeof(bool) },
                { "digged", typeof(bool) },
                { "itemCommentStatus", typeof(bool) },
                { "showNotPass", typeof(bool) },
                { "vl1", typeof(bool) },
                { "itemMute", typeof(bool) },
                { "privateItem", typeof(bool) },
                { "duetEnabled", typeof(bool) },
                { "stitchEnabled", typeof(bool) },
                { "shareEnabled", typeof(bool) },
                { "isAd", typeof(bool) },

                { "createTime", typeof(int) },
                { "duetDisplay", typeof(int) },
                { "stitchDisplay", typeof(int) },

                { "stats", typeof(JObject) },
                { "video", typeof(JObject) },
                { "duetInfo", typeof(JObject) },
            };

            var videoFields = new Dictionary<string, Type>() {
                { "cover", typeof(string) },
                { "playAddr", typeof(string) },
                { "duration", typeof(int) },
            };

            var statsFields = new Dictionary<string, Type>() {
                { "diggCount", typeof(long) },
                { "shareCount", typeof(long) },
                { "commentCount", typeof(long) },
                { "playCount", typeof(long) },
            };

            var duetFields = new Dictionary<string, Type>() {
                { "duetFromId", typeof(string) },
            };

            var result = topLevelFields.All(x => TryField(post, x, logger))
                         && videoFields.All(x => TryField((JObject) post["video"], x, logger))
                         && statsFields.All(x => TryField((JObject) post["stats"], x, logger))
                         && duetFields.All(x => TryField((JObject) post["duetInfo"], x, logger));

            if (result == false) {
                logger.Error("🔴🔴🔴 ERROR 🔴🔴🔴 This payload does not contain the bare minimum! Skipping!");
            }

            return result;
        }
    }
}
