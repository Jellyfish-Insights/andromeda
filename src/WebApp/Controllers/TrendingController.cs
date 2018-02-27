using ApplicationModels;
using ApplicationModels.Models.DataViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Extensions;

namespace WebApp.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class TrendingController : ControllerBase {
        private ApplicationDbContext _context;
        private ContentDataController _dataController;
        private IContentDataBackend _dataBackend;

        public TrendingController(
            ApplicationDbContext context,
            ContentDataController dataController,
            IContentDataBackend dataBackend
            ) {
            _context = context;
            _dataController = dataController;
            _dataBackend = dataBackend;
        }

        private static List<string> AcceptedSortingColumns = new List<string>(){
            "views",
            "reactions",
        };

        [HttpGet("[action]")]
        public ActionResult<TrendingResult> TopK(
            [FromQuery(Name = "k")] int size = 10,
            [FromQuery(Name = "from")] DateTime? dateStart = null,
            [FromQuery(Name = "until")] DateTime? dateStop = null,
            [FromQuery(Name = "sort")] string sortMetric = "Views",
            [FromQuery(Name = "when")] string when = "custom"
            ) {
            sortMetric = sortMetric.ToLower();
            DateTime queryDateStart;
            DateTime queryDateStop;
            switch (when) {
                case "custom":
                    queryDateStart = dateStart ?? DateTime.UtcNow.Date.Subtract(new TimeSpan(15, 0, 0, 0));
                    queryDateStop = dateStop ?? queryDateStart.Subtract(new TimeSpan(-15, 0, 0, 0));
                    break;
                case "last_week":
                    queryDateStart = DateTime.UtcNow.Date.Subtract(new TimeSpan(7, 0, 0, 0));
                    queryDateStop = DateTime.UtcNow.Date;
                    break;
                case "yesterday":
                    queryDateStart = DateTime.UtcNow.Date.Subtract(new TimeSpan(1, 0, 0, 0));
                    queryDateStop = queryDateStart;
                    break;
                case "today":
                    queryDateStart = DateTime.UtcNow.Date;
                    queryDateStop = queryDateStart;
                    break;
                default:
                    return BadRequest($"Parameter 'when' must be one of: 'custom', 'last_week', 'today', 'yesterday'");
            }

            var queryRangeLength = (queryDateStop - queryDateStart).TotalDays;

            // the set of valid parameters is restricted in order to bound impact of query on the system
            if (size > 20 | size < 1) {
                return BadRequest("Parameter 'k' must be one of 1,2,...,20");
            }
            if (!AcceptedSortingColumns.Exists(x => x == sortMetric)) {
                return BadRequest($"Cannot sort on '{sortMetric}'. Accepted values: {string.Join(",", AcceptedSortingColumns)}");
            }
            if (queryRangeLength > 90) {
                return BadRequest("Selected period cannot be greater than 90 days");
            }
            if (queryRangeLength < 0) {
                return BadRequest("Invalid date range");
            }

            var metricList = _dataController.GetMetricList(
                DateUtilities.ToControllersInputFormat(queryDateStart),
                DateUtilities.ToControllersInputFormat(queryDateStop)
                );

            var scores = metricList
                             .Select(x => new { id = x.Id, score = x.TotalMetrics.SingleOrDefault(m => m.Type.ToLower() == sortMetric)?.Value ?? 0, m = x })
                             .OrderByDescending(x => x.score)
                             .Take(size);

            var dailyBreakDown = _dataBackend.VideoMetricByDay(
                scores.Select(x => Convert.ToInt32(x.id)),
                queryDateStart,
                queryDateStop
                );

            var videoInfo = _dataController.GetVideoList();

            var queryResult = (
                from s in scores
                join v in videoInfo on s.id equals v.Id
                select new VideoTrendingInfo {
                Title = v.Title,
                DatePublished = DateUtilities.ToRestApiDateFormat(v.PublishedAt.Date),
                Tags = Clean(v.Tags),
                Urls = Clean(v.Sources),
                Total = AcceptedSortingColumns.
                            ToDictionary(
                    x => x,
                    x => s.m.TotalMetrics
                        .SingleOrDefault(m => m.Type.ToLower() == x)
                        ?.Value
                    ),
                DailyBreakDown = dailyBreakDown[Convert.ToInt32(s.id)]
            }).ToList();

            if (!queryResult.Any()) {
                return BadRequest($"No '{sortMetric}' on the selected period");
            }

            return Ok(new TrendingResult {
                From = DateUtilities.ToRestApiDateFormat(queryDateStart),
                Until = DateUtilities.ToRestApiDateFormat(queryDateStop),
                K = size,
                Sort = sortMetric,
                TopK = queryResult,
            });
        }

        private Dictionary<string, List<string>> Clean(List<Tag> tags) {
            return tags
                       .GroupBy(x => x.Type)
                       .ToDictionary(
                x => x.Key,
                x => x.Select(y => y.Value).ToList()
                );
        }

        private Dictionary<string, List<string>> Clean(List<Source> sources) {
            var flatLinkData = sources
                                   .ConcatMap(x => {
                var f = x.SourceObjects
                            .Where(s => s.Type == SourceObjectType.Video)
                            .ConcatMap(y => y.Links
                                           .Where(t => t.Type == SourceLinkType.Content)
                                           .Select(z => (source: x.SourceName, link: z.Link))
                                       );
                return f;
            });
            return flatLinkData.
                       GroupBy(x => x.source)
                       .ToDictionary(
                x => x.Key,
                x => x.Select(y => y.link).ToList()
                );
        }
    }

    public class TrendingResult {
        public String From;
        public String Until;
        public int K;
        public string Sort;
        public List<VideoTrendingInfo> TopK;
    }

    public class VideoTrendingInfo {
        public String Title;
        public string DatePublished;
        public Dictionary<string, List<string>> Tags;
        public Dictionary<string, List<string>> Urls;
        public Dictionary<string, double?> Total;
        public Dictionary<string, Dictionary<string, double>> DailyBreakDown;
    }
}
