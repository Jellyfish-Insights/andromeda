using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using DataLakeModels;
using DataLakeModels.Models.TikTok;
using Jobs.Fetcher.TikTok.Helpers;

namespace Jobs.Fetcher.TikTok {

    public class PostsQuery : AbstractTikTokFetcher {

        public PostsQuery(List<string> userIds): base(userIds) {}

        public override List<string> Dependencies() {
            return new List<string>() {};
        }

        private readonly object MyLock = new object();

        private int RetrievedInFull = 0;
        private int MissingSuperfluous = 0;
        private int MissingEssential = 0;

        private void FoundData(string fieldName, string postId) {
            Logger.Information($"🟢 id = {postId} - '{fieldName}' retrieved in full!");
            lock (MyLock) RetrievedInFull++;
        }

        private void MissingSuperfluousData(string fieldName, string postId) {
            Logger.Warning($"🟡 id = {postId} - could not retrieve '{fieldName}', it won't be saved");
            lock (MyLock) MissingSuperfluous++;
        }

        private void MissingEssentialData(string fieldName, string postId) {
            Logger.Error($"🔴 id = {postId} - could not retrieve essential field '{fieldName}', skipping this post");
            lock (MyLock) MissingEssential++;
        }

        private int AllPosts = 0;
        private int Success = 0;

        public void Work(JObject post,
                         string username
                         ) {
            // this function can be run multithreaded, but currently doesn't,
            // as the performance was measured to be equivalent, no matter how
            // many threads
            using (var dbContext = new DataLakeTikTokContext()) {
                if (!ApiDataFetcher.DoWeHaveTheBareMinimum(post, Logger)) {
                    Logger.Error("This payload does not contain the bare minimum! Skipping");
                    MissingEssentialData("entirePost", "");
                    return;
                }

                var id = post["id"].ToObject<string>();
                Logger.Information($"Found TikTok Post for '{username}', ID = '{id}'");

                Video newVideo;
                try {
                    var videoJson = (JObject) post["video"];
                    newVideo = ApiDataFetcher.GetTikTokVideoFromJson(videoJson, id, Logger);
                    if (newVideo == null) {
                        MissingEssentialData("video", id);
                        return;
                    } else {
                        DbWriter.WriteVideo(newVideo, dbContext, Logger);
                    }
                }catch (Exception exc) {
                    Logger.Error($"Unknown error: \n\n{exc.ToString()}");
                    MissingEssentialData("video", id);
                    return;
                }

                Author newAuthor = null;
                try {
                    if (post.TryGetValue("author", out var authorName) && authorName.ToString() == username.Substring(1)) {
                        newAuthor = ApiDataFetcher.GetTikTokAuthorFromPostJson(post);
                        Logger.Warning($"🟡 id = {id} - Author data exists, but is incomplete");
                    } else if (post.TryGetValue("author", out var authorJson)) {
                        newAuthor = ApiDataFetcher.GetTikTokAuthorFromAuthorJson(authorJson);
                        Logger.Information($"🟢 id = {id} - Author data retrieved in full!");
                    } else {
                        MissingEssentialData("author", id);
                        return;
                    }

                    DbWriter.WriteAuthor(newAuthor, dbContext, Logger);
                }catch (Exception exc) {
                    Logger.Error($"Unknown error: \n\n{exc.ToString()}");
                    MissingEssentialData("author", id);
                    return;
                }

                // end of essential data
                // everything underneath is optional and might be absent in the payload

                Music newMusic = null;
                try {
                    if (post.TryGetValue("music", out var musicJson)) {
                        newMusic = ApiDataFetcher.GetTikTokMusicFromJson(musicJson);
                        DbWriter.WriteMusic(newMusic, dbContext, Logger);
                        FoundData("music", id);
                    } else {
                        MissingSuperfluousData("music", id);
                    }
                }catch (Exception exc) {
                    Logger.Error($"Unknown error: \n\n{exc.ToString()}");
                    MissingSuperfluousData("music", id);
                }

                var newChallenges = new List<Challenge>();
                try {
                    if (post.TryGetValue("challenges", out var challengesJson)) {
                        newChallenges = ApiDataFetcher.GetTikTokChallengesFromJson(challengesJson);
                        DbWriter.WriteChallenges(newChallenges, dbContext, Logger);
                        FoundData("challenges", id);
                    } else {
                        MissingSuperfluousData("challenges", id);
                    }
                }catch (Exception exc) {
                    Logger.Error($"Unknown error: \n\n{exc.ToString()}");
                    MissingSuperfluousData("challenges", id);
                }

                var newTags = new List<Tag>();
                try {
                    if (post.TryGetValue("textExtra", out var tagsJson)) {
                        newTags = ApiDataFetcher.GetTikTokTagsFromJson(tagsJson);
                        DbWriter.WriteTags(newTags, dbContext, Logger);
                        FoundData("tags", id);
                    } else {
                        MissingSuperfluousData("tags", id);
                    }
                }catch (Exception exc) {
                    Logger.Error($"Unknown error: \n\n{exc.ToString()}");
                    MissingSuperfluousData("tags", id);
                }

                var newEffectStickers = new List<EffectSticker>();
                try {
                    if (post.TryGetValue("effectStickers", out var stickersJson)) {
                        newEffectStickers = ApiDataFetcher.GetTikTokEffectStickersFromJson(stickersJson);
                        DbWriter.WriteEffectStickers(newEffectStickers, dbContext, Logger);
                        FoundData("effectStickers", id);
                    } else {
                        MissingSuperfluousData("effectStickers", id);
                    }
                }catch (Exception exc) {
                    Logger.Error($"Unknown error: \n\n{exc.ToString()}");
                    MissingSuperfluousData("effectStickers", id);
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
                    DbWriter.WritePost(newPost, dbContext, Logger);
                    FoundData("post", id);
                }catch (Exception exc) {
                    Logger.Error($"Unknown error: \n\n{exc.ToString()}");
                    Logger.Error($"🔴 id = {id} - could not assemble 'newPost', all information found will be discarded!");
                    return;
                }

                try {
                    if (post.TryGetValue("authorStats", out var authorStatsJson)) {
                        var newAuthorStats = ApiDataFetcher.GetTikTokAuthorStatsFromJson((JObject) authorStatsJson, newAuthor, newPost.CreateTime);
                        DbWriter.WriteAuthorStats(newAuthorStats, dbContext, Logger);
                        FoundData("authorStats", id);
                    } else {
                        MissingSuperfluousData("authorStats", id);
                    }
                }catch (Exception exc) {
                    Logger.Error($"Unknown error: \n\n{exc.ToString()}");
                    MissingSuperfluousData("authorStats", id);
                }

                try {
                    if (post.TryGetValue("stats", out var statsJson)) {
                        var newPostStats = ApiDataFetcher.GetTikTokPostStatsJson(statsJson, newPost, newPost.CreateTime);
                        DbWriter.WritePostStats(newPostStats, dbContext, Logger);
                        FoundData("stats", id);
                    } else {
                        MissingSuperfluousData("stats", id);
                    }
                }catch (Exception exc) {
                    Logger.Error($"Unknown error: \n\n{exc.ToString()}");
                    MissingSuperfluousData("stats", id);
                }

                lock (MyLock) Success++;
            }
        }

        void ResetStatistics() {
            AllPosts = 0;
            Success = 0;
            RetrievedInFull = 0;
            MissingSuperfluous = 0;
            MissingEssential = 0;
        }

        public override void RunBody(string username) {
            var jobStart = DateTime.UtcNow;
            ResetStatistics();

            if (!DatabaseManager.TikTokScraperTablesExist()) {
                Logger.Warning($"TikTok Scraper tables do not exist.");
                Logger.Warning("If you are running a local test without the scraper, "
                               + "try importing DB with the commands \n\n"
                               + "pg_dump -d data_lake -t general_scraper_v1.video_info > video_info.sql"
                               + "psql -d data_lake < video_info.sql"
                               + "\n\n");
                return;
            }

            var authorId = DatabaseManager.GetTikTokId(username.Substring(1));
            var lastFetch = authorId != null? DatabaseManager.GetLastFetch(authorId) : DateTime.MinValue;
            var rows = DatabaseManager.GetRowCount(username, lastFetch);

            Logger.Information($"There are {rows} rows to process");

            // don't raise this too high, or you might have RAM issues
            // Remember every payload can have ~100 KB
            const int maxBatchSize = 100;

            int lastOffset = 0;
            while (true) {
                var partial = DatabaseManager
                                  .GetPayload(username, lastFetch, lastOffset, maxBatchSize)
                                  .Select(x => JObject.Parse(x))
                                  .ToList();
                AllPosts += partial.Count();

                var bareMinimum = partial
                                      .Where(x => ApiDataFetcher.DoWeHaveTheBareMinimum(x, Logger))
                                      .ToList();

                var count = bareMinimum.Count();
                if (count == 0)
                    break;

                lastOffset += count;
                Logger.Information($"Fetched {count} entries from DB, last = {lastOffset}");
                foreach (var post in bareMinimum)
                    Work(post, username);
            }

            Logger.Information($"🟢🟢🟢 Summary for {username} 🟢🟢🟢");

            Logger.Information($"We read {AllPosts} posts. Of those, {Success} "
                               + "contained all of the essential fields.");

            var arr = new List<string>() {
                "",
                String.Format("{0,30}: {1,6}", "Retrieved in full", RetrievedInFull),
                String.Format("{0,30}: {1,6}", "Missing superfluous data", MissingSuperfluous),
                String.Format("{0,30}: {1,6}", "Missing essential data", MissingEssential),
                ""
            };

            Logger.Information(String.Join("\n", arr));

            Logger.Information($"Deleting from temporary table for {username}...");
            DatabaseManager.DeleteFromTemporaryTable(username, jobStart);
        }
    }
}
