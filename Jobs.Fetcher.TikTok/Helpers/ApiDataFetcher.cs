using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using DataLakeModels.Models.TikTok;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;

using Andromeda.Common;
using Andromeda.Common.Extensions;


namespace Jobs.Fetcher.TikTok.Helpers {

    public static class ApiDataFetcher {
        public static JToken GetPosts() {
            JObject allPosts = JObject.Parse(File.ReadAllText(@"test.json"));
            return allPosts["itemList"];
        }
        public static Video GetTikTokVideoFromJSON(JToken videoJSON){
            var shareCovers = new List<string>();
            foreach(var shareCover in videoJSON["shareCover"]){
                shareCovers.Add(shareCover.ToString());
            }
            var newVideo = new Video() {
                Id = videoJSON["id"].ToString(),
                Height = videoJSON["height"].ToObject<int>(),
                Width = videoJSON["width"].ToObject<int>(),
                Duration = videoJSON["duration"].ToObject<int>(),
                Ratio = videoJSON["ratio"].ToString(),
                Cover = videoJSON["cover"].ToString(),
                OriginCover = videoJSON["originCover"].ToString(),
                DynamicCover = videoJSON["dynamicCover"].ToString(),
                PlayAddress = videoJSON["playAddr"].ToString(),
                DownloadAddress = videoJSON["downloadAddr"].ToString(),
                ShareCover = shareCovers,
                ReflowCover = videoJSON["reflowCover"].ToString(),
                BitRate = videoJSON["bitrate"].ToObject<int>(),
                EncodedType = videoJSON["encodedType"].ToString(),
                Format = videoJSON["format"].ToString(),
                VideoQuality = videoJSON["videoQuality"].ToString(),
                EncodedUserTag = videoJSON["encodeUserTag"].ToString(),
                CodecType = videoJSON["codecType"].ToString(),
                Definition = videoJSON["definition"].ToString()
            };

            return newVideo;
        }
        public static Music GetTikTokMusicFromJSON(JToken musicJSON){
            return new Music() {
                Id = musicJSON["id"].ToString(),
                Title = musicJSON["title"].ToString(),
                PlayUrl = musicJSON["playUrl"].ToString(),
                CoverThumb = musicJSON["coverThumb"].ToString(),
                CoverMedium = musicJSON["coverMedium"].ToString(),
                CoverLarge = musicJSON["coverLarge"].ToString(),
                AuthorName = musicJSON["authorName"].ToString(),
                Original = musicJSON["original"].ToObject<bool>(),
                Duration = musicJSON["duration"].ToObject<int>(),
                Album = musicJSON["album"].ToString()
            };
        }
        public static List<Challenge> GetTikTokChallengesFromJson(JToken challengesJSON){
            var newChallenges = new List<Challenge>();
            foreach(var challenge in challengesJSON){
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
        public static List<Tag> GetTikTokTagsFromJSON(JToken tagsJSON){
            var newTags = new List<Tag>();
            foreach(var tag in tagsJSON){  
                newTags.Add( new Tag() {
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
        public static List<EffectSticker> GetTikTokEffectStickersFromJSON(JToken effectStickersJSON){
            var effectStickers = new List<EffectSticker>();
            foreach(var sticker in effectStickersJSON){
                var newEffectSticker = new EffectSticker() {
                    Name = sticker["name"].ToString(),
                    Id = sticker["ID"].ToString()
                };
                effectStickers.Add(newEffectSticker);
            }
            return effectStickers;
        }
        public static Author GetTikTokAuthorFromJSON(JToken authorJSON){
            return new Author() {
                Id = authorJSON["id"].ToString(),
                UniqueId = authorJSON["uniqueId"].ToString(),
                Nickname = authorJSON["nickname"].ToString(),
                AvatarThumbnail = authorJSON["avatarThumb"].ToString(),
                AvatarMedium = authorJSON["avatarMedium"].ToString(),
                AvatarLarge = authorJSON["avatarLarger"].ToString(),
                Signature = authorJSON["signature"].ToString(),
                Verified = authorJSON["verified"].ToObject<bool>(),
                SecurityUId = authorJSON["secUid"].ToString(),
                Secret = authorJSON["secret"].ToObject<bool>(),
                FTC = authorJSON["ftc"].ToObject<bool>(),
                Relation = authorJSON["relation"].ToObject<int>(),
                OpenFavorite = authorJSON["openFavorite"].ToObject<int>(),
                CommentSetting = authorJSON["commentSetting"].ToObject<int>(),
                DuetSetting = authorJSON["duetSetting"].ToObject<int>(),
                StitchSetting = authorJSON["stitchSetting"].ToObject<int>(),
                PrivateAccount = authorJSON["privateAccount"].ToObject<bool>()
            };
        }
        public static AuthorStats GetTikTokAuthorStatsFromJSON(JToken authorStatsJSON, Author author, DateTime fetchTime){
            return new AuthorStats() {
                    FollowingCount =  authorStatsJSON["followingCount"].ToObject<long>(),
                    FollowerCount =  authorStatsJSON["followerCount"].ToObject<long>(),
                    HeartCount =  authorStatsJSON["heartCount"].ToObject<long>(),
                    VideoCount =  authorStatsJSON["videoCount"].ToObject<long>(),
                    DiggCount =  authorStatsJSON["diggCount"].ToObject<long>(),
                    Heart =  authorStatsJSON["heart"].ToObject<long>(),
                    ValidityStart = fetchTime,
                    ValidityEnd = DateTime.MaxValue,
                    AuthorId = author.Id,
                    Author = author
                };
        }
        public static Post GetTikTokPostFromJSON(JToken postJSON, Author author){
            return new Post() {
                Id = postJSON["id"].ToString(),
                CreateTime = new DateTime(1970,1,1,0,0,0,0,System.DateTimeKind.Utc).AddSeconds( postJSON["createTime"].ToObject<int>() ),
                DuetInfo = postJSON["duetInfo"]["duetFromId"].ToString(),
                OriginalItem = postJSON["originalItem"].ToObject<bool>(),
                OfficialItem = postJSON["officalItem"].ToObject<bool>(),
                Secret = postJSON["secret"].ToObject<bool>(),
                ForFriend = postJSON["forFriend"].ToObject<bool>(),
                Digged = postJSON["digged"].ToObject<bool>(),
                ItemCommentStatus = postJSON["itemCommentStatus"].ToObject<int>(),
                ShowNotPass = postJSON["showNotPass"].ToObject<bool>(),
                VL1 = postJSON["vl1"].ToObject<bool>(),
                ItemMute = postJSON["itemMute"].ToObject<bool>(),
                Private = postJSON["privateItem"].ToObject<bool>(),
                DuetEnabled = postJSON["duetEnabled"].ToObject<bool>(),
                StitchEnabled = postJSON["stitchEnabled"].ToObject<bool>(),
                ShareEnabled = postJSON["shareEnabled"].ToObject<bool>(),
                IsAd = postJSON["isAd"].ToObject<bool>(),
                DuetDisplay = postJSON["duetDisplay"].ToObject<int>(),
                StitchDisplay = postJSON["stitchDisplay"].ToObject<int>(),
                Author = author
            };
        }
        public static PostStats GetTikTokPostStatsJSON(JToken postStatsJSON, Post post, DateTime fetchTime){
            return new PostStats() {
                    DiggCount = postStatsJSON["diggCount"].ToObject<long>(),
                    ShareCount = postStatsJSON["shareCount"].ToObject<long>(),
                    CommentCount = postStatsJSON["commentCount"].ToObject<long>(),
                    PlayCount = postStatsJSON["playCount"].ToObject<long>(),
                    ValidityStart = fetchTime,
                    ValidityEnd = DateTime.MaxValue,
                    PostId = post.Id,
                    Post = post
                };
        }
    }
}
