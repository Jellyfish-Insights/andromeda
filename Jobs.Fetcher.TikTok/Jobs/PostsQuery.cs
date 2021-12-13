using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog.Core;
using DataLakeModels;
using Jobs.Fetcher.TikTok.Helpers;

namespace Jobs.Fetcher.TikTok {

    public class PostsQuery : AbstractTikTokFetcher {

        public PostsQuery(List<string> userIds): base(clients) {}

        public override List<string> Dependencies() {
            return new List<string>();
        }

        public override void RunBody(string userId) {
            using (var dbContext = new DataLakeTikTokContext()) {
                //Create a second loop for each TikTok client. Check with Erik and Victor if the Get option will receive an ID or username
                var logger = GetLogger();
                foreach(var post in ApiDataFetcher.GetPosts()){
                    var newAuthor = ApiDataFetcher.GetTikTokAuthorFromJSON(post["author"]);
                    DbWriter.WriteAuthor(newAuthor, dbContext, logger);

                    var newMusic = ApiDataFetcher.GetTikTokMusicFromJSON(post["music"]);
                    DbWriter.WriteMusic(newMusic, dbContext, logger);

                    var newChallenges = ApiDataFetcher.GetTikTokChallengesFromJson(post["challenges"]);
                    DbWriter.WriteChallenges(newChallenges, dbContext, logger);

                    var newTags = ApiDataFetcher.GetTikTokTagsFromJSON(post["textExtra"]);
                    DbWriter.WriteTags(newTags, dbContext, logger);

                    var newEffectStickers = ApiDataFetcher.GetTikTokEffectStickersFromJSON(post["effectStickers"]);
                    DbWriter.WriteEffectStickers(newEffectStickers, dbContext, logger);

                    var newVideo = ApiDataFetcher.GetTikTokVideoFromJSON(post["video"], post["id"].ToString());
                    DbWriter.WriteVideo(newVideo, dbContext, logger);

                    var challengeIds = new List<string>();
                    foreach(var challenge in newChallenges){
                        challengeIds.Add(challenge.Id);
                    }
                    var tagIds = new List<string>();
                    foreach(var tag in newTags){
                        tagIds.Add(tag.HashtagId);
                    }
                    var effectStickerIds = new List<string>();
                    foreach(var effectSticker in newEffectStickers){
                        effectStickerIds.Add(effectSticker.Id);
                    }
                    var newPost = ApiDataFetcher.GetTikTokPostFromJSON(post, newAuthor, newVideo, newMusic, challengeIds, tagIds, effectStickerIds);
                    DbWriter.WritePost(newPost, dbContext, logger);

                    var newAuthorStats = ApiDataFetcher.GetTikTokAuthorStatsFromJSON(post["authorStats"], newAuthor, newPost.CreateTime);
                    DbWriter.WriteAuthorStats(newAuthorStats, dbContext, logger);

                    var newPostStats = ApiDataFetcher.GetTikTokPostStatsJSON(post["stats"], newPost, newPost.CreateTime);
                    DbWriter.WritePostStats(newPostStats, dbContext, logger);

                }
            }
        }

    }
}
