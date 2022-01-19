// using System;
// using System.Collections.Generic;
// using System.Linq;
// using DataLakeModels;
// using DataLakeModels.Models;
// using DataLakeModels.Models.YouTube.Studio;
// using Serilog.Core;

// namespace Jobs.Fetcher.YouTube.Helpers {

//     public static class DbWriter {

//         private static Modified compareOldAndNew(
//             Video storedObj, Video newObj
//         ) {
//             if (storedObj == null)
//                 return Modified.New;

//             if (!storedObj.Equals(newObj)) {
//                 return Modified.Updated;
//             }
//             return Modified.Equal;
//         }

//         public static Modified compareOldAndNew(
//             IEnumerable<Video> storedObjs, IEnumerable<Video> newObjs
//         ) {
//             if (storedObjs == null)
//                 return Modified.New;

//             if (!storedObjs.ToHashSet().SetEquals(newObjs)) {
//                 return Modified.Updated;
//             }
//             return Modified.Equal;
//         }

//         public static void Write(IEnumerable<Video> videos, string channelId, Logger logger) {
//             using (var dlContext = new DataLakeYouTubeStudioContext()) {
//                 var now = DateTime.UtcNow;
//                 foreach (var newObj in videos) {
//                     var storedObj = dlContext.Videos.SingleOrDefault(v => v.VideoId == newObj.VideoId && v.ValidityStart <= now && now < v.ValidityEnd);

//                     newObj.ValidityEnd = DateTime.MaxValue;
//                     newObj.ValidityStart = DateTime.UtcNow;
//                     newObj.ChannelId = channelId;

//                     var modified = compareOldAndNew(storedObj, newObj);
//                     switch (modified) {
//                         case Modified.New:
//                             logger.Debug("Found new video: {VideoId}", newObj.VideoId);
//                             dlContext.Add(newObj);
//                             break;
//                         case Modified.Updated:
//                             logger.Debug("Found update to: {VideoId}", newObj.VideoId);
//                             storedObj.ValidityEnd = newObj.ValidityStart;
//                             dlContext.Update(storedObj);
//                             dlContext.Add(newObj);
//                             break;
//                         default:
//                             break;
//                     }
//                 }
//                 dlContext.SaveChanges();
//             }
//         }
//     }
// }
