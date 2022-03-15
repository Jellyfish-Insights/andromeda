using System;
using System.Collections.Generic;


namespace Jobs.Fetcher.YouTube.Helpers {
/* This data structure will help to parallelize requests */
    public class ViewerPercentagesTask {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ChannelId { get; set; }
        public string VideoId { get; set; }

        public override string ToString() {
            var lines = new List<string>() {
                "",
                $"Channel {ChannelId}",
                $"Video {VideoId}",
                $"StartDate {StartDate}",
                $"EndDate {EndDate}",
                ""
            };
            return String.Join("\n", lines);
        }
    }
}
