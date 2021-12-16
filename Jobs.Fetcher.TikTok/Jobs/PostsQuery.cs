using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog.Core;
using DataLakeModels;
using Jobs.Fetcher.TikTok.Helpers;

namespace Jobs.Fetcher.TikTok {

    public class PostsQuery : AbstractTikTokFetcher {

        //public UserQuery(Dictionary<string, ITwitterClient> clients): base(clients) {}

        public override List<string> Dependencies() {
            return new List<string>();
        }

        public override void RunBody() {
            using (var dbContext = new DataLakeTikTokContext()) {
                //Create a second loop for each TikTok client. Check with Erik and Victor if the Get option will receive an ID or username
                var logger = GetLogger();
                foreach(var post in ApiDataFetcher.GetPosts()){
                    var newAuthor = ApiDataFetcher.GetTikTokAuthorFromJSON(post["author"]);
                    DbWriter.WriteAuthor(newAuthor, dbContext, logger);

                    var newPost = ApiDataFetcher.GetTikTokPostFromJSON(post, newAuthor);
                    DbWriter.WritePost(newPost, dbContext, logger);

                    var newVideo = ApiDataFetcher.GetTikTokVideoFromJSON(post["video"]);
                    DbWriter.WriteVideo(newVideo, dbContext, logger);

                    var newMusic = ApiDataFetcher.GetTikTokMusicFromJSON(post["music"]);
                    DbWriter.WriteMusic(newMusic, dbContext, logger);

                    var newChallenges = ApiDataFetcher.GetTikTokChallengesFromJson(post["challenges"]);
                    DbWriter.WriteChallenges(newChallenges, dbContext, logger);

                    var newTags = ApiDataFetcher.GetTikTokTagsFromJSON(post["textExtra"]);
                    DbWriter.WriteTags(newTags, dbContext, logger);

                    var newEffectStickers = ApiDataFetcher.GetTikTokEffectStickersFromJSON(post["effectStickers"]);
                    DbWriter.WriteEffectStickers(newEffectStickers, dbContext, logger);

                    var newAuthorStats = ApiDataFetcher.GetTikTokAuthorStatsFromJSON(post["authorStats"], newAuthor, newPost.CreateTime);
                    DbWriter.WriteAuthorStats(newAuthorStats, dbContext, logger);

                    var newPostStats = ApiDataFetcher.GetTikTokPostStatsJSON(post["stats"], newPost, newPost.CreateTime);
                    DbWriter.WritePostStats(newPostStats, dbContext, logger);
                    
                }
            }
        }

    }
}
