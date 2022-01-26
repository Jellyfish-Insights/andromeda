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

        public static DateTime EpochToDateTime(uint epochSeconds)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(epochSeconds);
        }

        public static Video DTOToVideo (Video_DTO dto)
        {
            var validityStart = DateTime.UtcNow;
            var validityEnd = DateTime.MaxValue;
            var dateMeasure = EpochToDateTime(dto.DateMeasure);

            return new Video {
                ValidityStart = validityStart,
                ValidityEnd = validityEnd,
                DateMeasure = dateMeasure,
                ChannelId = dto.ChannelId,
                VideoId = dto.VideoId,
                Metric = dto.Metric,
                Value = dto.Value
            };
        }

        public static void Write(IEnumerable<Video_DTO> videoDTOs, Logger logger) {
            using (var dlContext = new DataLakeYouTubeStudioContext()) {
                foreach (var videoDTO in videoDTOs) {
                    var newObj = DTOToVideo(videoDTO);
                    var storedObj = dlContext.Videos.SingleOrDefault(v =>
                        v.VideoId == newObj.VideoId
                        && v.DateMeasure.ToUniversalTime().Date == newObj.DateMeasure.ToUniversalTime().Date
                        && v.Metric == newObj.Metric
                        && v.ValidityEnd > DateTime.UtcNow
                    );

                    var modified = compareOldAndNew(storedObj, newObj);
                    switch (modified) {
                        case Modified.New:
                            logger.Debug("Found new video: {VideoId}", newObj.VideoId);
                            dlContext.Add(newObj);
                            break;
                        case Modified.Updated:
                            logger.Debug("Found update to: {VideoId}", newObj.VideoId);
                            storedObj.ValidityEnd = newObj.ValidityStart;
                            dlContext.Update(storedObj);
                            dlContext.Add(newObj);
                            break;
                        default:
                            break;
                    }
                    dlContext.SaveChanges();
                }

            }
        }
    }
}
