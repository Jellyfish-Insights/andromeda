using System;
using System.Collections.Generic;
using System.Linq;
using DataLakeModels;
using DataLakeModels.Models;
using DataLakeModels.Models.YouTube.Studio;
using Serilog.Core;

namespace Jobs.Fetcher.YouTubeStudio.Helpers {

    public static class DbWriter {

        private static Modified compareOldAndNew(
            Video storedObj, Video newObj
            ) {
            if (storedObj == null)
                return Modified.New;

            if (!storedObj.Equals(newObj)) {
                return Modified.Updated;
            }
            return Modified.Equal;
        }

        public static DateTime EpochToDateTime(uint epochSeconds) {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(epochSeconds);
        }

        public static Video DTOToVideo(Video_DTO dto) {
            var validityStart = DateTime.UtcNow;
            var validityEnd = DateTime.MaxValue;
            var eventTime = EpochToDateTime(dto.EventDate);

            return new Video {
                       ValidityStart = validityStart,
                       ValidityEnd = validityEnd,
                       EventDate = eventTime.ToUniversalTime().Date,
                       ChannelId = dto.ChannelId,
                       VideoId = dto.VideoId,
                       Metric = dto.Metric,
                       Value = dto.Value
            };
        }

        public static void WriteDTOs(IEnumerable<Video_DTO> videoDTOs, Logger logger) {
            foreach (var videoDTO in videoDTOs) {
                var video = DTOToVideo(videoDTO);
                WriteVideo(video, logger);
            }
        }

        public static void WriteVideo(Video video, Logger logger) {
            using (var dlContext = new DataLakeYouTubeStudioContext()) {
                var storedObj = dlContext.Videos.SingleOrDefault(v =>
                                                                 v.VideoId == video.VideoId
                                                                 && v.EventDate.ToUniversalTime().Date == video.EventDate.ToUniversalTime().Date
                                                                 && v.Metric == video.Metric
                                                                 && v.ValidityEnd > DateTime.UtcNow
                                                                 );

                var modified = compareOldAndNew(storedObj, video);
                switch (modified) {
                    case Modified.New:
                        logger.Debug("Found new video: {VideoId}", video.VideoId);
                        dlContext.Add(video);
                        break;
                    case Modified.Updated:
                        logger.Debug("Found update to: {VideoId}", video.VideoId);
                        storedObj.ValidityEnd = video.ValidityStart;
                        dlContext.Update(storedObj);
                        dlContext.Add(video);
                        break;
                    default:
                        break;
                }
                dlContext.SaveChanges();
            }
        }
    }
}
