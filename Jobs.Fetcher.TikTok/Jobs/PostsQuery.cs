using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog.Core;
using DataLakeModels;
using Jobs.Fetcher.TikTok.Helpers;
using DataLakeModels.Models.TikTok;
using Newtonsoft.Json.Linq;

namespace Jobs.Fetcher.TikTok {

    public class PostsQuery : AbstractTikTokFetcher {

        public PostsQuery(List<string> userIds): base(userIds) {}

        public override List<string> Dependencies() {
            return new List<string>() {};
        }

        public override void RunBody(string username) {
            using (var dbContext = new DataLakeTikTokContext()) {
                var logger = GetLogger();
                if (!DatabaseManager.TikTokScraperTablesExist()) {
                    Logger.Warning($"TikTok Scraper tables do not exist.");
                    return;
                }

                var authorId = DatabaseManager.GetTikTokId(username.Substring(1), dbContext);
                var lastFetch = authorId != null? DatabaseManager.GetLastFetch(authorId, dbContext) : DateTime.MinValue;
                foreach (var post in ApiDataFetcher.GetPosts(username, lastFetch, Logger)) {
                    if (!post.TryGetValue("id", out var id)) {
                        Logger.Error("Could not get post ID from JSON. Skipping this post.");
                        continue;
                    }
                    Logger.Information("Found TikTok Post for '" + username + "' of ID '" + id.ToString() + "'.");

                    Author newAuthor = null;
                    try {
                        if (post.TryGetValue("author", out var authorName) && authorName.ToString() == username.Substring(1)) {
                            Logger.Warning($"Author data is incomplete");
                            newAuthor = ApiDataFetcher.GetTikTokAuthorFromPostJson(post);
                        } else if (post.TryGetValue("author", out var authorJson)) {
                            newAuthor = ApiDataFetcher.GetTikTokAuthorFromAuthorJson(authorJson);
                        } else {
                            Logger.Error("Could not get author data from JSON. Skipping this post.");
                            continue;
                        }
                        DbWriter.WriteAuthor(newAuthor, dbContext, logger);
                    }catch (Exception) {
                        Logger.Error("Could not get author data.");
                        throw;
                    }

                    Music newMusic = null;
                    try {
                        if (post.TryGetValue("music", out var musicJson)) {
                            newMusic = ApiDataFetcher.GetTikTokMusicFromJson(musicJson);
                            DbWriter.WriteMusic(newMusic, dbContext, logger);
                        } else {
                            Logger.Warning("Could not get music data from JSON. Continuing.");
                        }
                    }catch (Exception) {
                        Logger.Error("Could not get music data.");
                        throw;
                    }

                    var newChallenges = new List<Challenge>();
                    try {
                        if (post.TryGetValue("challenges", out var challengesJson)) {
                            newChallenges = ApiDataFetcher.GetTikTokChallengesFromJson(challengesJson);
                            DbWriter.WriteChallenges(newChallenges, dbContext, logger);
                        } else {
                            Logger.Warning("Could not get challenges data from JSON. Continuing.");
                        }
                    }catch (Exception) {
                        Logger.Error("Could not get challenges data.");
                        throw;
                    }

                    var newTags = new List<Tag>();
                    try {
                        if (post.TryGetValue("textExtra", out var tagsJson)) {
                            newTags = ApiDataFetcher.GetTikTokTagsFromJson(tagsJson);
                            DbWriter.WriteTags(newTags, dbContext, logger);
                        } else {
                            Logger.Warning("Could not get tags data from JSON. Continuing.");
                        }
                    }catch (Exception) {
                        Logger.Error("Could not get tags data.");
                        throw;
                    }

                    var newEffectStickers = new List<EffectSticker>();
                    try {
                        if (post.TryGetValue("effectStickers", out var stickersJson)) {
                            newEffectStickers = ApiDataFetcher.GetTikTokEffectStickersFromJson(stickersJson);
                            DbWriter.WriteEffectStickers(newEffectStickers, dbContext, logger);
                        } else {
                            Logger.Warning("Could not get effect stickers data from JSON. Continuing.");
                        }
                    }catch (Exception) {
                        Logger.Error("Could not get effect stickers data.");
                        throw;
                    }

                    Video newVideo = null;
                    try {
                        if (post.TryGetValue("video", out var videoJson)) {
                            newVideo = ApiDataFetcher.GetTikTokVideoFromJson((JObject) videoJson, id.ToString());
                            DbWriter.WriteVideo(newVideo, dbContext, logger);
                        } else {
                            Logger.Error("Could not get video data from JSON. Skipping this post.");
                            continue;
                        }
                    }catch (Exception) {
                        Logger.Error("Could not get video data.");
                        throw;
                    }

                    var challengeIds = new List<string>();
                    foreach (var challenge in newChallenges) {
                        challengeIds.Add(challenge.Id);
                    }
                    var tagIds = new List<string>();
                    foreach (var tag in newTags) {
                        tagIds.Add(tag.HashtagId);
                    }
                    var effectStickerIds = new List<string>();
                    foreach (var effectSticker in newEffectStickers) {
                        effectStickerIds.Add(effectSticker.Id);
                    }

                    Post newPost = null;
                    try {
                        newPost = ApiDataFetcher.GetTikTokPostFromJson(post, newAuthor, newVideo, newMusic, challengeIds, tagIds, effectStickerIds);
                        DbWriter.WritePost(newPost, dbContext, logger);
                    }catch (NullReferenceException) {
                        Logger.Error("Could not get post data from JSON. Skipping this post.");
                        continue;
                    }catch (Exception) {
                        Logger.Error("Could not get post data.");
                        throw;
                    }

                    try {
                        if (post.TryGetValue("authorStats", out var authorStatsJson)) {
                            var newAuthorStats = ApiDataFetcher.GetTikTokAuthorStatsFromJson(authorStatsJson, newAuthor, newPost.CreateTime);
                            DbWriter.WriteAuthorStats(newAuthorStats, dbContext, logger);
                        } else {
                            Logger.Warning("Could not get author stats data from JSON. Continuing.");
                        }
                    }catch (Exception) {
                        Logger.Error("Could not get author stats data.");
                        throw;
                    }

                    try {
                        if (post.TryGetValue("stats", out var statsJson)) {
                            var newPostStats = ApiDataFetcher.GetTikTokPostStatsJson(statsJson, newPost, newPost.CreateTime);
                            DbWriter.WritePostStats(newPostStats, dbContext, logger);
                        } else {
                            Logger.Warning("Could not get post stats data from JSON. Continuing.");
                        }
                    }catch (Exception) {
                        Logger.Error("Could not get post stats data.");
                        throw;
                    }
                }
            }
        }
    }
}
