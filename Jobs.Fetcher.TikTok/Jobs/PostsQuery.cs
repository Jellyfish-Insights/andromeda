using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog.Core;
using DataLakeModels;
using Jobs.Fetcher.TikTok.Helpers;

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

                    DataLakeModels.Models.TikTok.Author newAuthor = null;
                    try{
                        if (post["author"].ToString() == username.Substring(1)) {
                            Logger.Warning($"Author data is incomplete");
                            newAuthor = ApiDataFetcher.GetTikTokAuthorFromPostJson(post);
                        } else {
                            newAuthor = ApiDataFetcher.GetTikTokAuthorFromAuthorJson(post["author"]);
                        }
                        DbWriter.WriteAuthor(newAuthor, dbContext, logger);
                    }catch(Exception e){
                        Logger.Error(&"Could not get author data.");
                        return;
                    }

                    try{
                        var newMusic = ApiDataFetcher.GetTikTokMusicFromJson(post["music"]);
                        DbWriter.WriteMusic(newMusic, dbContext, logger);
                    }catch(Exception e){
                        Logger.Error(&"Could not get music data.");
                        return;
                    }

                    try{
                        var newChallenges = ApiDataFetcher.GetTikTokChallengesFromJson(post["challenges"]);
                        DbWriter.WriteChallenges(newChallenges, dbContext, logger);
                    }catch(Exception e){
                        Logger.Error(&"Could not get challenges data.");
                        return;
                    }

                    try{
                        var newTags = ApiDataFetcher.GetTikTokTagsFromJson(post["textExtra"]);
                        DbWriter.WriteTags(newTags, dbContext, logger);
                    }catch(Exception e){
                        Logger.Error(&"Could not get tags data.");
                        return;
                    }

                    try{
                        var newEffectStickers = ApiDataFetcher.GetTikTokEffectStickersFromJson(post["effectStickers"]);
                        DbWriter.WriteEffectStickers(newEffectStickers, dbContext, logger);
                    }catch(Exception e){
                        Logger.Error(&"Could not get Effect Stickers data.");
                        return;
                    }

                    try{
                        var newVideo = ApiDataFetcher.GetTikTokVideoFromJson(post["video"], post["id"].ToString());
                        DbWriter.WriteVideo(newVideo, dbContext, logger);
                    }catch(Exception e){
                        Logger.Error(&"Could not get video data.");
                        return;
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

                    try{
                        var newPost = ApiDataFetcher.GetTikTokPostFromJson(post, newAuthor, newVideo, newMusic, challengeIds, tagIds, effectStickerIds);
                        DbWriter.WritePost(newPost, dbContext, logger);
                    }catch(Exception e){
                        Logger.Error(&"Could not get post data.");
                        return;
                    }

                    try{
                        var newAuthorStats = ApiDataFetcher.GetTikTokAuthorStatsFromJson(post["authorStats"], newAuthor, newPost.CreateTime);
                        DbWriter.WriteAuthorStats(newAuthorStats, dbContext, logger);
                    }catch(Exception e){
                        Logger.Error(&"Could not get author stats data.");
                        return;
                    }

                    try{
                        var newPostStats = ApiDataFetcher.GetTikTokPostStatsJson(post["stats"], newPost, newPost.CreateTime);
                        DbWriter.WritePostStats(newPostStats, dbContext, logger);
                    }catch(Exception e){
                        Logger.Error(&"Could not get post stats data.");
                        return;
                    }
                }
            }
        }
    }
}
