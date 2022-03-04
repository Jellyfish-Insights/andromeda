using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog.Core;
using DataLakeModels;
using Jobs.Fetcher.TikTok.Helpers;
using DataLakeModels.Models.TikTok;

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
                    Logger.Information("Found TikTok Post for '" + username + "' of ID '" + post["id"] + "'.");

                    Author newAuthor = null;
                    try {
                        if (post["author"].ToString() == username.Substring(1)) {
                            Logger.Warning($"Author data is incomplete");
                            newAuthor = ApiDataFetcher.GetTikTokAuthorFromPostJson(post);
                        } else {
                            newAuthor = ApiDataFetcher.GetTikTokAuthorFromAuthorJson(post["author"]);
                        }
                        DbWriter.WriteAuthor(newAuthor, dbContext, logger);
                    }catch (NullReferenceException) {
                        Logger.Error("Could not get author data. Skipping this post.");
                        continue;
                    }catch (Exception) {
                        Logger.Error("Could not get author data.");
                        throw;
                    }

                    Music newMusic = null;
                    try {
                        newMusic = ApiDataFetcher.GetTikTokMusicFromJson(post["music"]);
                        DbWriter.WriteMusic(newMusic, dbContext, logger);
                    }catch (NullReferenceException) {
                        Logger.Error("Could not get music data. Continuing.");
                    }catch (Exception) {
                        Logger.Error("Could not get music data.");
                        throw;
                    }

                    var newChallenges = new List<Challenge>();
                    try {
                        newChallenges = ApiDataFetcher.GetTikTokChallengesFromJson(post["challenges"]);
                        DbWriter.WriteChallenges(newChallenges, dbContext, logger);
                    }catch (NullReferenceException) {
                        Logger.Error("Could not get challenges data. Continuing.");
                    }catch (Exception) {
                        Logger.Error("Could not get challenges data.");
                        throw;
                    }

                    var newTags = new List<Tag>();
                    try {
                        newTags = ApiDataFetcher.GetTikTokTagsFromJson(post["textExtra"]);
                        DbWriter.WriteTags(newTags, dbContext, logger);
                    }catch (NullReferenceException) {
                        Logger.Error("Could not get tags data. Continuing.");
                    }catch (Exception) {
                        Logger.Error("Could not get tags data.");
                        throw;
                    }

                    var newEffectStickers = new List<EffectSticker>();
                    try {
                        newEffectStickers = ApiDataFetcher.GetTikTokEffectStickersFromJson(post["effectStickers"]);
                        DbWriter.WriteEffectStickers(newEffectStickers, dbContext, logger);
                    }catch (NullReferenceException) {
                        Logger.Error("Could not get effect stickers data. Continuing.");
                    }catch (Exception) {
                        Logger.Error("Could not get effect stickers data.");
                        throw;
                    }

                    Video newVideo = null;
                    try {
                        newVideo = ApiDataFetcher.GetTikTokVideoFromJson(post["video"], post["id"].ToString());
                        DbWriter.WriteVideo(newVideo, dbContext, logger);
                    }catch (NullReferenceException) {
                        Logger.Error("Could not get video data. Skipping this post.");
                        continue;
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
                        Logger.Error("Could not get post data. Skipping this post.");
                        continue;
                    }catch (Exception) {
                        Logger.Error("Could not get post data.");
                        throw;
                    }

                    try {
                        var newAuthorStats = ApiDataFetcher.GetTikTokAuthorStatsFromJson(post["authorStats"], newAuthor, newPost.CreateTime);
                        DbWriter.WriteAuthorStats(newAuthorStats, dbContext, logger);
                    }catch (NullReferenceException) {
                        Logger.Error("Could not get author stats data. Continuing.");
                    }catch (Exception) {
                        Logger.Error("Could not get author stats data.");
                        throw;
                    }

                    try {
                        var newPostStats = ApiDataFetcher.GetTikTokPostStatsJson(post["stats"], newPost, newPost.CreateTime);
                        DbWriter.WritePostStats(newPostStats, dbContext, logger);
                    }catch (NullReferenceException) {
                        Logger.Error("Could not get post stats data. Continuing.");
                    }catch (Exception) {
                        Logger.Error("Could not get post stats data.");
                        throw;
                    }
                }
            }
        }
    }
}
