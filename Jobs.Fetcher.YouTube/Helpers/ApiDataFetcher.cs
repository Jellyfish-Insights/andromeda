using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataLakeModels;
using YTD = DataLakeModels.Models.YouTube.Data;
using YTA = DataLakeModels.Models.YouTube.Analytics;
using Google.Apis.Requests;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.YouTubeAnalytics.v2;
using Google.Apis.YouTubeAnalytics.v2.Data; // EmptyResponse
using Serilog.Core;
using Andromeda.Common;
using Andromeda.Common.Extensions;
using System.Threading;

namespace Jobs.Fetcher.YouTube.Helpers {
    public class ApiDataFetcher {

        protected readonly Logger _logger;
        private readonly DateTime _now;
        protected readonly YouTubeService _dataService;
        protected readonly YouTubeAnalyticsService _analyticsService;

        private Nullable<DateTime> _mostRecentFetch = null;

        // This is approximately when YouTube started
        private static readonly DateTime _dateOrigin = new DateTime(2005, 01, 01);

        private HashSet<string> _rejected = new HashSet<string>();

        public ApiDataFetcher(
                    Logger logger,
                    YouTubeService dataService,
                    YouTubeAnalyticsService analyticsService
                             ) {
            _now = DateTime.UtcNow;

            _logger = logger;
            _dataService = dataService;
            _analyticsService = analyticsService;
            _mostRecentFetch = DbReader.FindMostRecentRecord();
        }

        /*##############################################################################*/
        /*# KEEPING TRACK OF REQUESTS ##################################################*/
        /*##############################################################################*/

        protected static readonly object YTDLock = new object();
        protected static int YTDRequests = 0;
        protected static readonly object YTALock = new object();
        protected static int YTARequests = 0;

        // Types are in the namespace Google.Apis.Request, common to YouTube Data
        // and YouTube Analytics
        private T GetResponse<T>(ClientServiceRequest<T> r) where T : IDirectResponseSchema {
            if ( r.GetType().BaseType.GetGenericTypeDefinition().IsAssignableFrom(typeof(YouTubeBaseServiceRequest<>)) ) {
                lock (YTDLock) {
                    YTDRequests++;
                    if (YTDRequests % 100 == 0)
                        _logger.Debug($"\n*** QUOTA ***\n{YTDRequests++} requests sent to YouTube Data\n");
                }
            }
            else if ( r.GetType().BaseType.GetGenericTypeDefinition().IsAssignableFrom(typeof(YouTubeAnalyticsBaseServiceRequest<>)) ) {
                lock (YTALock) {
                    YTARequests++;
                    if (YTARequests % 100 == 0)
                        _logger.Debug($"\n*** QUOTA ***\n{YTARequests++} requests sent to YouTube Analytics\n");
                }
            }
            var startTime = DateTime.UtcNow;
            try {
                var response = r.ExecuteAsync().Result;
                return response;
            }
            catch (Exception exc) {
                if (exc.InnerException is Google.GoogleApiException) {
                    _logger.Error($"Google API raised an error!\n{exc.ToString()}");
                    SlowDown();
                } else {
                    _logger.Error($"An unknown exception was raised!\n{exc.ToString()}");
                }
                return default(T); // in effect, returns null
            }
            finally {
                var finishTime = DateTime.UtcNow;
                RespectQuota(startTime, finishTime);
            }
        }

        public void PrintAPIStatistics() {
            lock (YTDLock) {
                lock (YTALock) {
                    _logger.Information($"\n\nApplication (not job!) statistics:\n\n"
                    + $"{YTDRequests} requests sent to YT Data, "
                    + $"{YTARequests} requests sent to YT Analytics.\n\n");
                }
            }
        }

        /*##############################################################################*/
        /*# CONCURRENCY CONTROL ########################################################*/
        /*##############################################################################*/

        // On top of the value provided by YouTube, we take a 20% margin for security
        // Keep in mind that there are overheads not accounted for, so our margin is
        // even larger than that

        // On the other hand, there is, besides a minute limit, a daily limit. We
        // cannot exceed 100k requests per day (about 10% of the limit by minute).
        // Thus, we need to make sure the fetchers are not running for more than
        // 140 minutes per day at full capacity
        const double _maxRequestsMinute = 720.0;
        const double _maxRequestsMinuteSafe = _maxRequestsMinute * 0.80;
        const double _maxRequestsSecond = _maxRequestsMinuteSafe / 60.0;
        private double _minMillisecondsPerRequest = 1.0 / _maxRequestsSecond * 1000.0 * 1;

        private double slowDownFactor = 1.0;
        private Nullable<DateTime> lastSlowDown = null;
        private readonly object slowDownLock = new object();
        public void UseThreads(int nThreads) {
            _minMillisecondsPerRequest = 1.0 / _maxRequestsSecond * 1000.0 * nThreads;
        }

        // This function will likely be called in a burst. In other words, not only
        // one thread will call it, but all at the same time. We have to use a lock
        // to stop the other threads, and thus avoid this being called too many times
        // in a sequence

        // To be honest, if we get to the point this function is called,
        // there will be very little we can do. If you see this is a regular issue,
        // you should probably decrease the factor at _maxRequestsMinuteSafe
        // or use less threads
        public void SlowDown() {
            double sleepMs = 60.0 * 1000.0;
            double slowDownMinIntervalMs = sleepMs * 1.10;

            lock (slowDownLock) {
                _logger.Warning("Slowing down...");

                var now = DateTime.UtcNow;
                if (lastSlowDown != null)
                    if ((lastSlowDown.Value - now).TotalMilliseconds > slowDownMinIntervalMs) {
                        slowDownFactor *= 1.15;
                    }

                lastSlowDown = now;
                if (slowDownFactor > 2.00) {
                    // at the rate of 1.15 per slow down, we would have to have around
                    // 5 slow down bursts to get to this point
                    throw new ApplicationException("Too slow!");
                }
                Thread.Sleep((int) sleepMs);
            }
        }

        public void RespectQuota(DateTime startTime, DateTime finishTime, string info = "") {

            var elapsedMilliseconds = (finishTime - startTime).TotalMilliseconds;
            _logger.Verbose($"{info} Request finished in {(int) elapsedMilliseconds} ms");

            if (elapsedMilliseconds < _minMillisecondsPerRequest * slowDownFactor) {
                double diff = _minMillisecondsPerRequest - elapsedMilliseconds;
                _logger.Verbose($"{info} Will sleep for {(int) diff} milliseconds");
                Thread.Sleep((int) diff);
            }
        }

        public string EstimateCompletionTime(int requests) {
            const double overheadFactor = 1.05;
            double seconds = (double) requests / _maxRequestsMinuteSafe * 60.0 * overheadFactor;
            int minutes = (int) seconds / 60;
            int remainingSeconds = (int) seconds - minutes * 60;
            return $"{minutes}min {remainingSeconds}sec";
        }

        public void Diagnostics() {
            // not thread safe, but should also not be run concurrently
            var nFailed = _rejected.Count();
            if (nFailed > 0) {
                string failed = String.Join(" ", _rejected);
                _logger.Information($"{nFailed} videos failed: \n{failed}\n");
                return;
            }
            _logger.Information("All videos fetched successfully!");
        }

        public void RejectedAdd(string videoId) {
            lock(_rejected) _rejected.Add(videoId);
        }

        public void RejectedUnionWith(List<string> videoIdList) {
            lock(_rejected) _rejected.UnionWith(videoIdList);
        }

        public HashSet<string> RejectedGet() {
            lock(_rejected) return _rejected;
        }

        public void ResetRejected() {
            lock(_rejected) _rejected.Clear();
        }

        private readonly object dbLastDateLock = new object();

        /*##############################################################################*/
        /*# FETCHERS ###################################################################*/
        /*##############################################################################*/

        public (string, string) FetchChannelInfo() {
            var request = _dataService.Channels.List("id,contentDetails");
            request.Mine = true;
            var result = GetResponse(request);

            if (result == null) {
                _logger.Error("Could not retrieve channel info!");
                throw new ApplicationException("Could not retrieve channel info!");
            }

            var channelId = result.Items[0].Id;
            var uploadsListId = result.Items[0].ContentDetails.RelatedPlaylists.Uploads;

            return (channelId, uploadsListId);
        }

        public IEnumerable<string> FetchVideoIds(string uploadsListId) {

            int pageNumber = 0;
            var request = _dataService.PlaylistItems.List("snippet");
            request.PlaylistId = uploadsListId;

            PlaylistItemListResponse response;
            do {
                _logger.Verbose($"#{pageNumber + 1} page of results");
                response = GetResponse(request);

                if (response == null || response.Items == null || response.Items.Count() == 0) {
                    _logger.Warning("Zero video ids returned!");
                    yield break;
                }

                foreach (var videoId in response.Items.Select(i => i.Snippet.ResourceId.VideoId)) {
                    yield return videoId;
                }

                request.PageToken = response.NextPageToken;
                pageNumber++;
            } while (!String.IsNullOrEmpty(response.NextPageToken));
        }

        // Initial value here was 50, but YouTube can serve up to 500 videos at
        // once. Since we now have the option to parallelize this in the caller
        // function, I increased the value
        private const int BatchSize = 150;

        public List<Video> FetchVideoProperties(IEnumerable<string> videoIds) {
            var batches = IEnumerableExtensions.SplitIntoBatches(videoIds, BatchSize).ToList();
            _logger.Verbose($"We have {videoIds.Count()} video ids, {batches.Count()} batches");

            var output = new List<Video>();

            for (int i = 0; i < batches.Count(); i++) {
                var batch = batches[i];
                var partialOutput = new List<Video>();
                try {
                    var request = _dataService.Videos.List("snippet,contentDetails,status");
                    request.Id = String.Join(',', batch);

                    var response = GetResponse(request);
                    if (response == null) {
                        _logger.Error($"Batch {i}: Failed to fetch video properties!");
                        continue;
                    }

                    foreach (var video in response.Items) {
                        if (video != null) {
                            partialOutput.Add(video);
                        } else {
                            _logger.Error($"Video is null.");
                        }
                    }
                    output.AddRange(partialOutput);
                }
                catch (Exception) {
                    _logger.Error($"An exception happened. That's all we know.");
                    RejectedUnionWith(batch);
                }
            }

            return output;
        }

        public IEnumerable<Video> FetchVideoStatistics(IEnumerable<string> videoIds) {
            var request = _dataService.Videos.List("statistics");
            var batches = IEnumerableExtensions.SplitIntoBatches<string>(videoIds, BatchSize).ToList();
            for (int i = 0; i < batches.Count(); i++) {
                var batch = batches[i];

                request.Id = String.Join(',', batch);
                var response = GetResponse(request);
                if (response == null) {
                    _logger.Error($"Batch {i}: Failed to fetch video statistics!");
                    continue;
                }

                foreach (var video in response.Items) {
                    yield return video;
                }
            }
        }

        public IEnumerable<Playlist> FetchPlaylists(string channelId) {
            var playlistRequest = _dataService.Playlists.List("snippet");
            playlistRequest.ChannelId = channelId;

            PlaylistListResponse response;
            do {
                response = GetResponse(playlistRequest);
                if (response == null) {
                    _logger.Error("Failed to fetch next page of playlists!");
                    yield break;
                }

                foreach (var playList in response.Items) {
                    yield return playList;
                }

                playlistRequest.PageToken = response.NextPageToken;
            } while (!String.IsNullOrEmpty(response.NextPageToken));
        }

        private IEnumerable<PlaylistItem> FetchItemsInPlaylist(string playListId) {
            var playlistItemsRequest = _dataService.PlaylistItems.List("snippet");
            playlistItemsRequest.PlaylistId = playListId;

            PlaylistItemListResponse result;
            do {
                result = GetResponse(playlistItemsRequest);
                if (result == null) {
                    _logger.Error("Failed to fetch next page from items in playlist!");
                    yield break;
                }

                foreach (var item in result.Items) {
                    yield return item;
                }

                playlistItemsRequest.PageToken = result.NextPageToken;
            } while (!String.IsNullOrEmpty(result.NextPageToken));
        }

        private (DateTime, DateTime) GetStartDateEndDate(bool reprocess) {
            DateTime startDate, endDate;
            if (reprocess) {
                _logger.Information("Reprocessing was chosen, we'll fetch all data we can.");
                startDate = _dateOrigin;
                endDate = _now;
            } else {
                startDate = _mostRecentFetch ?? _dateOrigin;
                if ((_now - startDate).Days > 365) {
                    _logger.Warning("You haven't fetched your data in a while. This run "
                        + "will be limited to the first year since your last fetch. "
                        + "Please run again later.");
                    endDate = startDate + new TimeSpan(365, 0, 0);
                }
                else {
                    endDate = _now;
                }
            }
            return (startDate, endDate);
        }

        public string[] GetVideoIdsInPlaylist(string playlistId) {
            return FetchItemsInPlaylist(playlistId)
                       .Where(x => x.Snippet.ResourceId.Kind == "youtube#video")
                       .Select(x => x.Snippet.ResourceId.VideoId)
                       .Distinct()
                       .ToArray();
        }

        private IEnumerable<IList<object>> FetchVideoDailyMetrics(
                                            string channelId,
                                            YTD.Video video,
                                            bool reprocess
                                            ) {

            DateTime startDate, endDate;
            (startDate, endDate) = GetStartDateEndDate(reprocess);

            var reportRequest = _analyticsService.Reports.Query();
            reportRequest.Ids = $"channel=={channelId}";
            reportRequest.StartDate = startDate.ToString("yyyy-MM-dd");
            reportRequest.EndDate = endDate.ToString("yyyy-MM-dd");
            reportRequest.Metrics = "views,likes,shares,comments,averageViewDuration,dislikes,subscribersGained,subscribersLost,videosAddedToPlaylists,videosRemovedFromPlaylists";
            reportRequest.Filters = $"video=={video.VideoId}";
            reportRequest.Dimensions = "day";
            reportRequest.Sort = "day";

            var report = GetResponse(reportRequest);
            if (report == null || report.Rows == null || report.Rows.Count() == 0) {
                _logger.Error($"Report failed for video {video.VideoId}!");
                yield break;
            }

            _logger.Verbose("Found {Rows} rows", report.Rows.Count());

            foreach (var row in report.Rows) {
                yield return row;
            }
        }

        public List<(DateTime date, long subscriberViews)> FetchSubscriberViews(
                                                string channelId,
                                                YTD.Video video,
                                                bool reprocess
                                                ) {

            DateTime startDate, endDate;
            (startDate, endDate) = GetStartDateEndDate(reprocess);

            var reportRequest = _analyticsService.Reports.Query();
            reportRequest.Ids = $"channel=={channelId}";
            reportRequest.StartDate = startDate.ToString("yyyy-MM-dd");
            reportRequest.EndDate = endDate.ToString("yyyy-MM-dd");
            reportRequest.Metrics = "views";
            reportRequest.Filters = $"video=={video.VideoId};subscribedStatus==SUBSCRIBED";
            reportRequest.Dimensions = "subscribedStatus,day";

            var returnRows = new List<(DateTime, long)>();

            var report = GetResponse(reportRequest);
            if (report == null) {
                _logger.Error($"Could not get Subscriber Views for video {video.VideoId}");
                RejectedAdd(video.VideoId);
                return returnRows;
            }

            if (report.Rows == null) {
                _logger.Warning("Got empty response!");
                RejectedAdd(video.VideoId);
            }
            else if (report.Rows.Count() == 0) {
                _logger.Verbose($"Response exists but zero rows, video = {video.VideoId}");
            }
            else {
                _logger.Verbose($"Obtained {report.Rows.Count()} rows");
                returnRows.AddRange( report.Rows.Select(
                    row => {
                        var date = Convert.ToDateTime(row[1]).Date;
                        var subscriberViews = (long) row[2];
                        return (date, subscriberViews);
                    }
                ));
            }

            return returnRows;
        }

        public IEnumerable<YTA.VideoDailyMetric> FetchDailyMetrics(
                                                string channelId,
                                                YTD.Video video,
                                                bool reprocess = false
                                                ) {
            _logger.Debug($"Processing channel {channelId} , video {video.VideoId}");
            var dailyMetrics = FetchVideoDailyMetrics(channelId, video, reprocess);
            var subscriberViews = FetchSubscriberViews(channelId, video, reprocess);

            return dailyMetrics.Select(x =>
                Api2DbObjectConverter.ConvertDailyMetricRow(video.VideoId, x, subscriberViews));
        }

        private IList<IList<object>> RunViewerPercentageReport(ViewerPercentagesTask task) {
            var emptyResponse = (IList<IList<object>>)Enumerable.Empty<IList<object>>();

            if (task == null) {
                _logger.Warning("Received a null task object!");
                return emptyResponse;
            }

            var reportRequest = _analyticsService.Reports.Query();

            var parameters = new Dictionary<string, string> {
                { "StartDate", task.StartDate.ToString("yyyy-MM-dd") },
                { "EndDate", task.EndDate.ToString("yyyy-MM-dd") },
                { "Ids", $"channel=={task.ChannelId}" },
                { "Metrics", "viewerPercentage" },
                { "Filters", $"video=={task.VideoId}" },
                { "Dimensions", "gender,ageGroup" },
                { "Sort", "gender,ageGroup" }
            };

            reportRequest.StartDate = parameters["StartDate"];
            reportRequest.EndDate = parameters["EndDate"];
            reportRequest.Ids = parameters["Ids"];
            reportRequest.Metrics = parameters["Metrics"];
            reportRequest.Filters = parameters["Filters"];
            reportRequest.Dimensions = parameters["Dimensions"];
            reportRequest.Sort = parameters["Sort"];

            StringBuilder sb = new StringBuilder("We will send this request:\n");
            foreach (var item in parameters) {
                sb.Append(String.Format("\t{0,15} : {1}\n", item.Key, item.Value));
            }
            _logger.Verbose(sb.ToString());

            var result = GetResponse(reportRequest);
            if (result == null) {
                _logger.Error($"Request failed:\n{sb.ToString()}");
                RejectedAdd(task.VideoId);
                return emptyResponse;
            }

            var rows = result.Rows;
            _logger.Verbose($"Response contains {rows.Count()} rows");
            foreach (var row in rows) {
                _logger.Verbose(String.Join(", ", row));
            }

            return rows;
        }

        private Nullable<DateTime> GetViewerPercentagesLastDate(string videoId) {
            YTA.ViewerPercentageLastDate VPLD;
            using (var dbContext = new DataLakeYouTubeAnalyticsContext()) {
                VPLD = dbContext.ViewerPercentageLastDates
                                    .SingleOrDefault(x => x.VideoId == videoId);
            }
            return VPLD == null ? null : (Nullable<DateTime>) VPLD.Date;
        }

        private void UpsertViewerPercentagesLastDate(string videoId, DateTime newDate) {
            lock (dbLastDateLock) {
                var lastDate = GetViewerPercentagesLastDate(videoId);
                using (var dbContext = new DataLakeYouTubeAnalyticsContext()) {
                    if (lastDate == null) {
                        /* insert */
                        var newVPLD = new YTA.ViewerPercentageLastDate()
                                    { VideoId = videoId, Date = newDate };
                        dbContext.Add(newVPLD);
                    }
                    else {
                        /* update */
                        var existingVPLD = dbContext.ViewerPercentageLastDates
                                            .Where(x => x.VideoId == videoId)
                                            .Single();
                        existingVPLD.Date = newDate;
                    }
                    dbContext.SaveChanges();
                }
            }
        }

        public List<ViewerPercentagesTask> GetViewerPercentagesTasks(
                                                        string channelId,
                                                        YTD.Video video,
                                                        bool fetchAll
                                                        ) {
            var tasks = new List<ViewerPercentagesTask>();
            var videoId = video.VideoId;
            var publishedAt = video.PublishedAt;

            var lastDate = GetViewerPercentagesLastDate(videoId);

            if (lastDate == null) {
                // maybe the video simply doesn't support this report,
                // check for early termination
                var tmpTask = new ViewerPercentagesTask() {
                    ChannelId = channelId,
                    StartDate = publishedAt,
                    EndDate = _now,
                    VideoId = videoId
                };
                if (!RunViewerPercentageReport(tmpTask).Any()) {
                    RejectedAdd(videoId);
                    return tasks;
                }
            }

            DateTime rangeInitialDate;
            if (lastDate == null || fetchAll) {
                rangeInitialDate = publishedAt.Date;
            } else {
                rangeInitialDate = lastDate.Value.Date;
            }

            DateTime rangeEndDate = _now.Date;

            // limit fetch range to one year
            if (!fetchAll && (rangeEndDate - rangeInitialDate).Days > 365) {
                _logger.Warning($"Video {videoId}: the range is too long! "
                    + "Limiting task to first year of unfetched data.");
                rangeEndDate = rangeInitialDate + new TimeSpan(365, 0, 0, 0);
            }

            var daysInRange = DateHelper.DaysInRange(rangeInitialDate,
                                                     rangeEndDate,
                                                     false,
                                                     1)
                                            .ToList();


            foreach (var d in daysInRange) {
                var newTask = new ViewerPercentagesTask() {
                    ChannelId = channelId,
                    VideoId = videoId,
                    StartDate = rangeInitialDate,
                    EndDate = d
                };
                _logger.Verbose($"Adding task:\n{newTask.ToString()}");
                tasks.Add(newTask);
            }
            _logger.Information($"{tasks.Count()} tasks added for {videoId}");
            return tasks;
        }

        public void DoViewerPercentageTask(ViewerPercentagesTask task) {
            var viewerPercentages = RunViewerPercentageReport(task);

            if (viewerPercentages.Any()) {
                DbWriter.Write(
                    task.VideoId,
                    task.EndDate,
                    viewerPercentages.Select(x =>
                        Api2DbObjectConverter.ConvertViewerPercentageRow(x)),
                    _now
                    );
            }
            // even if we have no rows for the endDate, it is still a valid fetch
            // and we shouldn't have to redo it later
            UpsertViewerPercentagesLastDate(task.VideoId, task.EndDate);
        }
    }

    public class APIStressFetcher: ApiDataFetcher {
        public APIStressFetcher(
                    Logger logger,
                    YouTubeService dataService,
                    YouTubeAnalyticsService analyticsService
                    ): base(logger, dataService, analyticsService) {}

        public DateTime testStartTime {get; set;}
        public DateTime testEndTime {get; set;}

        public int GetYTARequests() {
            lock (YTALock) return YTARequests;
        }

        public int GetYTDRequests() {
            lock (YTDLock) return YTDRequests;
        }

        public void StressTestYTA(string channelId, string videoId, int tid) {
            _logger.Information($"YTA - Starting thread {tid}");
            // we want the request to succeed while returning the smallest possible
            // number of rows (even zero)
            var today = DateTime.UtcNow.Date;
            var lastWeek = today.AddDays(-7);

            var startDate = lastWeek.ToString("yyyy-MM-dd");
            var endDate = today.ToString("yyyy-MM-dd");

            while (true) {
                var reportRequest = _analyticsService.Reports.Query();
                reportRequest.Ids = $"channel=={channelId}";
                reportRequest.StartDate = startDate;
                reportRequest.EndDate = endDate;
                reportRequest.Metrics = "views";
                reportRequest.Filters = $"video=={videoId}";
                reportRequest.Dimensions = "day";
                reportRequest.Sort = "day";

                try {
                    var result = reportRequest.ExecuteAsync().Result;
                    lock (YTALock) {
                        YTARequests++;
                        if (YTARequests % 1000 == 0)
                            _logger.Information($"{YTARequests++} requests sent to YouTube Analytics");
                    }
                }
                catch (Exception exc) {
                    if (exc.InnerException is Google.GoogleApiException) {
                        _logger.Information($"YTA - Thread {tid} received GoogleApiException! Breaking!");
                        break;
                    }
                    else {
                        _logger.Error("Unknown error happened. Continuing!");
                    }
                }
                Thread.Sleep(5000);
            }
        }

        public void StressTestYTD(int tid) {
            _logger.Information($"YTD - Starting thread {tid}");

            while (true) {
                var request = _dataService.Channels.List("id,contentDetails");
                request.Mine = true;
                try {
                    var result = request.ExecuteAsync().Result;
                    lock (YTDLock) {
                        YTDRequests++;
                        if (YTDRequests % 1000 == 0)
                            _logger.Information($"{YTDRequests++} requests sent to YouTube Data");
                    }
                }
                catch (Exception exc) {
                    if (exc.InnerException is Google.GoogleApiException) {
                        _logger.Information($"YTD - Thread {tid} received GoogleApiException! Breaking!");
                        break;
                    }
                    else {
                        _logger.Error("Unknown error happened. Continuing!");
                    }
                }
            }
        }
    }
}
