using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Google.Apis.YouTube.v3;
using Google.Apis.YouTubeAnalytics.v2;
using Google.Apis.YouTubeAnalytics.v2.Data;

using Andromeda.Common.Jobs;
using Andromeda.Common.Logging;
using DataLakeModels;
using DL = DataLakeModels.Models.YouTube.Studio;

using Jobs.Fetcher.YouTube.Helpers;
using Jobs.Fetcher.YouTubeStudio.Helpers;

/*
 * To enable those jobs, you will need to add the following scopes to your list
 * of authorized scopes:

        https://www.googleapis.com/auth/youtube
        https://www.googleapis.com/auth/youtubepartner

 * See file utilities/youtube_credentials.py
 */

namespace Jobs.Fetcher.YouTubeStudio {
    public abstract class YTSGroupsAbstractJob : AbstractJob {
        public const string GroupIdentifier = "[JELLYFISH] ";
        public const int MaxItemsPerGroup = 500;
        public const int TimeToSleepMs = 1000;
        protected readonly List<YouTubeService> YTDList;
        protected readonly List<YouTubeAnalyticsService> YTAList;
        protected YouTubeService YTD;
        protected YouTubeAnalyticsService YTA;
        protected string ChannelId;
        protected string UploadsListId;

        protected ApiDataFetcher _fetcher;
        public YTSGroupsAbstractJob(
            List<(YouTubeService, YouTubeAnalyticsService)> services
            ) {
            YTDList = new List<YouTubeService>();
            YTAList = new List<YouTubeAnalyticsService>();
            foreach (var servicePair in services) {
                YTDList.Add(servicePair.Item1);
                YTAList.Add(servicePair.Item2);
            }
            Logger = GetLogger();
        }

        public override void Run() {
            for (int i = 0; i < YTDList.Count(); i++) {
                YTD = YTDList[i];
                YTA = YTAList[i];
                _fetcher = new ApiDataFetcher(Logger, YTD, YTA);
                (ChannelId, UploadsListId) = _fetcher.FetchChannelInfo();
                Logger.Information($"This job is now concerned with channel {ChannelId}");
                RunBody();
            }
        }

        protected abstract void RunBody();

        protected override Logger GetLogger() {
            return LoggerFactory.GetLogger<DataLakeLoggingContext>(Id());
        }

        /* API METHODS */

        protected void ReportError(Errors e) {
            if (e == null) {
                return;
            }
            Logger.Error($@"Response to API request contains an error:
						Code = {e.Code}
						Error = {e.Error}
						ETag = {e.ETag}
						RequestId = {e.RequestId}
						");
            Logger.Error("For more information, see https://googleapis.dev/dotnet/Google.Apis.YouTubeAnalytics.v2/latest/api/Google.Apis.YouTubeAnalytics.v2.Data.Errors.html");
            throw new Exception("Response to API request contains an error. See message above.");
        }

        protected string PostGroupToAPI(string groupTitle) {
            var group = new Group {
                ContentDetails = new GroupContentDetails {
                    ItemType = "youtube#video"
                },
                Snippet = new GroupSnippet {
                    Title = GroupIdentifier + groupTitle
                }
            };

            var insertRequest = YTA.Groups.Insert(group);
            var insertResponse = insertRequest.Execute();
            ReportError(insertResponse.Errors);

            Logger.Information($"A new group was created, with id {insertResponse.Id}");
            Thread.Sleep(TimeToSleepMs);
            return insertResponse.Id;
        }

        protected void PostGroupItemToAPI(string groupId, string itemId) {
            var body = new GroupItem {
                GroupId = groupId,
                Kind = "youtube#groupItem",
                Resource = new GroupItemResource {
                    Id = itemId,
                    Kind = "youtube#video"
                }
            };
            var request = YTA.GroupItems.Insert(body);
            var response = request.Execute();
            ReportError(response.Errors);
            Thread.Sleep(TimeToSleepMs);
        }

        protected void DeleteGroupFromAPI(string groupId) {
            var request = YTA.Groups.Delete();
            request.Id = groupId;
            var response = request.Execute();
            if (response != null) {
                throw new Exception("We were expecting an empty response!");
            }
            Thread.Sleep(TimeToSleepMs);
        }

        protected HashSet<string> GetAllVideoIdsFromAPI() {
            var videoIdsAPI = new HashSet<string>(
                _fetcher.FetchVideoIds(UploadsListId));
            Thread.Sleep(TimeToSleepMs);
            return videoIdsAPI;
        }

        protected HashSet<DL.Group> GetAllGroupsFromAPI() {
            var groups = new List<DL.Group>();
            string nextPageToken = "";
            do {
                var listRequest = YTA.Groups.List();
                listRequest.Mine = true;
                listRequest.Id = ""; /* This is really necessary :( */
                listRequest.PageToken = nextPageToken;

                var response = listRequest.Execute();
                ReportError(response.Errors);
                Thread.Sleep(TimeToSleepMs);
                foreach (var gr in response.Items) {
                    if (gr.Kind != "youtube#group") {
                        Logger.Warning("This is not a group!");
                        continue;
                    }
                    var newGroup = new DL.Group {
                        GroupId = gr.Id,
                        ChannelId = ChannelId,
                        Title = gr.Snippet.Title
                    };
                    groups.Add(newGroup);
                }
                nextPageToken = response.NextPageToken;
            }
            while (nextPageToken != null && nextPageToken != "");

            // This filter is needed because we the YouTube user might create
            // their own Analytics groups, but we are concerned solely with the
            // groups created by this application and saved with the starting
            // identifier in the title

            return new HashSet<DL.Group>(
                groups.Where(gr =>
                             gr.Title != null
                             && gr.Title.StartsWith(GroupIdentifier)
                             ));
        }

        protected List<String> GetItemIdsForGroupId(string groupId) {
            var request = YTA.GroupItems.List();
            request.GroupId = groupId;
            var response = request.Execute();
            ReportError(response.Errors);
            Thread.Sleep(TimeToSleepMs);

            var ids = new List<String>();
            foreach (var it in response.Items) {
                if (it.Resource.Kind != "youtube#video") {
                    Logger.Warning("This is not a video!");
                    continue;
                }
                ids.Add(it.Resource.Id);
            }
            return ids;
        }

        /* public method, may be useful for testing */
        public void DeleteAllGroupsFromAPI() {
            Logger.Information("Function DeleteAllGroupsFromAPI was called. "
                               + "Proceeding to delete all groups from YouTube Analytics.");
            var groups = GetAllGroupsFromAPI();
            foreach (var gr in groups) {
                DeleteGroupFromAPI(gr.GroupId);
            }
        }

        /* DB METHODS */

        protected HashSet<string> GetAllVideoIdsFromDB() {
            using (var dlContext = new DataLakeYouTubeStudioContext()) {
                return new HashSet<string>
                           (dlContext.Items
                               .Where(it => it.ChannelId == ChannelId)
                               .Select(it => it.ItemId));
            }
        }

        protected HashSet<DL.Group> GetAllGroupsFromDB() {
            using (var dlContext = new DataLakeYouTubeStudioContext()) {
                return new HashSet<DL.Group>
                           (dlContext.Groups.Where(gr => gr.ChannelId == ChannelId));
            }
        }

        protected void InsertGroupInDB(string groupId, string title) {
            using (var dlContext = new DataLakeYouTubeStudioContext()) {
                var now = DateTime.UtcNow;
                var newGroup = new DL.Group {
                    GroupId = groupId,
                    ChannelId = ChannelId,
                    Title = GroupIdentifier + title,
                    RegistrationDate = now,
                    UpdateDate = now
                };
                dlContext.Groups.Add(newGroup);
                dlContext.SaveChanges();
            }
        }

        protected void InsertItemInDB(string itemId) {
            using (var dlContext = new DataLakeYouTubeStudioContext()) {
                var now = DateTime.UtcNow;
                var it = new DL.Item {
                    ItemId = itemId,
                    ChannelId = ChannelId,
                    RegistrationDate = now,
                    UpdateDate = now,
                    Group = null,
                    GroupId = null
                };
                dlContext.Items.Add(it);
                dlContext.SaveChanges();
            }
        }

        protected HashSet<DL.Group> GetAvailableGroupsFromDB() {
            using (var dlContext = new DataLakeYouTubeStudioContext()) {
                return new HashSet<DL.Group>(
                    dlContext.Groups
                        .Where(gr =>
                               gr.ChannelId == ChannelId
                               && gr.Items.Count() < MaxItemsPerGroup
                               ));
            }
        }

        protected HashSet<DL.Item> GetOrphanItemsFromDB() {
            using (var dlContext = new DataLakeYouTubeStudioContext()) {
                return new HashSet<DL.Item>(
                    dlContext.Items
                        .Where(it =>
                               it.ChannelId == ChannelId
                               && it.Group == null
                               ));
            }
        }

        protected void AssociateGroupAndItemInDB(string groupId, string itemId) {
            Logger.Debug($"Associating item {itemId} to group {groupId}");
            using (var dlContext = new DataLakeYouTubeStudioContext()) {
                DL.Group gr;
                DL.Item item;
                try {
                    gr = dlContext.Groups.First(g => g.GroupId == groupId);
                    item = dlContext.Items.First(it => it.ItemId == itemId);
                } catch (Exception e) {
                    Logger.Error(e.ToString());
                    throw new Exception($"Could not find pair groupId = {groupId}, "
                                        + $"itemId = {itemId}");
                }
                if (gr.Items == null) {
                    gr.Items = new List<DL.Item>();
                }
                gr.Items.Add(item);
                dlContext.SaveChanges();
            }
        }

        protected void DetachItemFromGroupInDB(string itemId) {
            Logger.Debug($"Detaching item {itemId} from its group");
            using (var dlContext = new DataLakeYouTubeStudioContext()) {
                var item = dlContext.Items
                               .Where(it => it.ItemId == itemId)
                               .Single();
                item.Group = null;
                dlContext.SaveChanges();
            }
        }
    }

    public class Groups_EnsureAllItemsAreInDB : YTSGroupsAbstractJob {

        public Groups_EnsureAllItemsAreInDB(
            List<(YouTubeService, YouTubeAnalyticsService)> services
            ): base(services) {}

        public override List<string> Dependencies() {
            return new List<string>();
        }

        /* */

        protected override void RunBody() {
            HashSet<string> videoIdsAPI = GetAllVideoIdsFromAPI();
            HashSet<string> videoIdsDB = GetAllVideoIdsFromDB();
            HashSet<string> weNeedToAdd = new HashSet<string>
                                              (videoIdsAPI.Except(videoIdsDB));

            Logger.Information($"API said {videoIdsAPI.Count()} videos exist. "
                               + $"There were already {videoIdsDB.Count()} registered in DB. "
                               + $"We need to add {weNeedToAdd.Count()} videos.");

            foreach (var itemId in weNeedToAdd) {
                InsertItemInDB(itemId);
            }
        }
    }

    public class Groups_EnsureAllGroupsAreInDB : YTSGroupsAbstractJob {

        public Groups_EnsureAllGroupsAreInDB(
            List<(YouTubeService, YouTubeAnalyticsService)> services
            ): base(services) {}

        public override List<string> Dependencies() {
            return new List<string>();
        }

        /* */

        protected override void RunBody() {
            HashSet<DL.Group> groupsAPI = GetAllGroupsFromAPI();
            HashSet<DL.Group> groupsDB = GetAllGroupsFromDB();
            HashSet<DL.Group> weNeedToAdd = new HashSet<DL.Group>
                                                (groupsAPI.Except(groupsDB));

            Logger.Information($"API contains {groupsAPI.Count()} groups. "
                               + $"We have {groupsDB.Count()} in the DB. "
                               + $"We need to add {weNeedToAdd.Count()} groups.");

            foreach (var gr in weNeedToAdd) {
                InsertGroupInDB(gr.GroupId, gr.Title);
            }
        }
    }

    public class Groups_AssociateGroupsAndItems : YTSGroupsAbstractJob {

        public Groups_AssociateGroupsAndItems(
            List<(YouTubeService, YouTubeAnalyticsService)> services
            ): base(services) {}

        public override List<string> Dependencies() {
            return new List<string> {
                       IdOf<Groups_EnsureAllItemsAreInDB>(),
                       IdOf<Groups_EnsureAllGroupsAreInDB>()
            };
        }

        /* */
        protected override void RunBody() {
            var groupIds = GetAllGroupsFromDB().Select(gr => gr.GroupId);
            Logger.Information($"We are currently keeping track of "
                               + $"{groupIds.Count()} groups.");

            foreach (var groupId in groupIds) {
                var itemIds = GetItemIdsForGroupId(groupId);
                Logger.Information($"According to the API, {groupId} should "
                                   + "contain the following items:\n");
                foreach (var itemId in itemIds) {
                    Logger.Information($"\t{itemId}");
                    AssociateGroupAndItemInDB(groupId, itemId);
                }
            }
        }
    }

    public class Groups_InsertOrphanItems : YTSGroupsAbstractJob {

        public Groups_InsertOrphanItems(
            List<(YouTubeService, YouTubeAnalyticsService)> services
            ): base(services) {}

        public override List<string> Dependencies() {
            return new List<string> {
                       IdOf<Groups_AssociateGroupsAndItems>()
            };
        }

        /* */

        protected override void RunBody() {
            var orphanItems = GetOrphanItemsFromDB();
            var groups = GetAvailableGroupsFromDB();

            Logger.Information($"There are {orphanItems.Count()} orphan items");
            foreach (var orph in orphanItems) {
                Logger.Information($"\t{orph}");
            }

            while (orphanItems.Count() > 0) {
                groups = GetAvailableGroupsFromDB();
                Logger.Information($"There are {groups.Count()} available groups");
                foreach (var gr in groups) {
                    Logger.Debug(gr.ToString());
                }
                if (groups.Count() == 0) {
                    Logger.Information("There are no available groups. Creating a new one...");
                    string title = DateTime.UtcNow.ToString();
                    var groupId = PostGroupToAPI(title);
                    try {
                        InsertGroupInDB(groupId, title);
                    } catch (Exception e) {
                        Logger.Error(e.ToString());
                        DeleteGroupFromAPI(groupId);
                        throw new Exception("Could not insert group in DB!");
                    }
                    Logger.Debug($"New group id is {groupId}");
                    continue;
                }

                var popItemId = orphanItems.First().ItemId;
                var popGroupId = groups.First().GroupId;
                Logger.Information($"Adding item {popItemId} to group {popGroupId}");
                AssociateGroupAndItemInDB(popGroupId, popItemId);
                try {
                    PostGroupItemToAPI(popGroupId, popItemId);
                } catch (Exception e) {
                    Logger.Error(e.ToString());
                    DetachItemFromGroupInDB(popItemId);
                    throw new Exception("Could not post group item to API!");
                }

                orphanItems = GetOrphanItemsFromDB();
            }

            Logger.Information("All items belong now to a group!");
        }
    }
}
