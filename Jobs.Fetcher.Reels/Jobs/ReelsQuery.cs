using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog.Core;
using DataLakeModels;
using Jobs.Fetcher.Reels.Helpers;
using DataLakeModels.Models.Reels;

namespace Jobs.Fetcher.Reels {

    public class ReelsQuery : AbstractReelsFetcher {

        public ReelsQuery(List<string> userIds): base(userIds) {}

        public override List<string> Dependencies() {
            return new List<string>() {};
        }

        public override void RunBody(string username) {
            using (var dbContext = new DataLakeReelsContext()) {
                var logger = GetLogger();
                if (!DatabaseManager.GeneralScraperTablesExist()) {
                    Logger.Warning($"Reels Scraper tables do not exist.");
                    return;
                }

                var userId = DatabaseManager.GetReelsId(username, dbContext);
                Logger.Information($"Found ID of ' {username}': '{userId}'.");
                var lastFetch = userId != null? DatabaseManager.GetLastFetch(userId, dbContext) : DateTime.MinValue;
                foreach (var reelsJson in ApiDataFetcher.GetReels(username, lastFetch, Logger)) {
                    var id = reelsJson["id"].ToString();
                    //Logger.Information("Found Reels Post for '" + username + "' of ID '" + reelsJson["id"] + "'.");
                    Logger.Information($"Found Reels Post for ' {username}' of ID '{id}'.");

                    User newUser = null;
                    try {
                        newUser = ApiDataFetcher.GetReelsUserFromJson(reelsJson["user"]);
                        DbWriter.WriteUser(newUser, dbContext, logger);
                    }catch (Exception e) {
                        Logger.Error($"Could not get User data. Reels ID: {id}");
                        throw e;
                    }

                    Reel newReel = null;
                    try {
                        newReel = ApiDataFetcher.GetReelsFromJson(reelsJson, newUser.Pk);
                        DbWriter.WriteReel(newReel, dbContext, logger);
                    }catch (Exception e) {
                        Logger.Error($"Could not get Reel's data. Reels ID: {id}");
                        throw e;
                    }

                    ImageVersion newImageVersion = null;
                    try {
                        newImageVersion = ApiDataFetcher.GetReelsImageVersionFromJson(reelsJson["image_versions2"], id);
                        DbWriter.WriteImageVersion(newImageVersion, dbContext, logger);
                    }catch (Exception e) {
                        Logger.Error($"Could not get Image Version data. Reels ID: {id}");
                        throw e;
                    }

                    var newImages = new List<Image>();
                    try {
                        foreach (var image in reelsJson["image_versions2"]["candidates"]) {
                            newImages.Add(ApiDataFetcher.GetReelsImageFromJson(image, newImageVersion.Id));
                        }
                        DbWriter.WriteImages(newImages, dbContext, logger);
                    }catch (Exception e) {
                        Logger.Error($"Could not get Images data. Reels ID: {id}");
                        throw e;
                    }

                    AnimatedThumbnail newThumbnail = null;
                    try {
                        newThumbnail = ApiDataFetcher.GetReelsAnimatedThumbnailFromJson(reelsJson["image_versions2"]["animated_thumbnail_spritesheet_info_candidates"]["default"], newImageVersion.Id);
                        DbWriter.WriteAnimatedThumbnail(newThumbnail, dbContext, logger);
                    }catch (Exception) {
                        Logger.Error($"Could not get Animated Thumbnail data. Reels ID: {id}");
                        //throw e;
                    }

                    Caption newCaption = null;
                    try {
                        newCaption = ApiDataFetcher.GetReelsCaptionFromJson(reelsJson["caption"], id);
                        DbWriter.WriteCaption(newCaption, dbContext, logger);
                    }catch (Exception e) {
                        Logger.Error($"Could not get Caption data. Reels ID: {id}");
                        throw e;
                    }

                    ClipsMeta newClipsMeta = null;
                    try {
                        newClipsMeta = ApiDataFetcher.GetReelsClipsFromJson(reelsJson["clips_metadata"], id);
                        DbWriter.WriteClipsMeta(newClipsMeta, dbContext, logger);
                    }catch (Exception) {
                        Logger.Error($"Could not get Clips Meta data. Reels ID: {id}");
                        //throw e;
                    }

                    var newComments = new List<CommentInfo>();
                    try {
                        foreach (var comment in reelsJson["comments"]) {
                            newComments.Add(ApiDataFetcher.GetReelsCommentFromJson(comment, id));
                        }
                        DbWriter.WriteCommentInfo(newComments, dbContext, logger);
                    }catch (Exception e) {
                        Logger.Error($"Could not get Comments data. Reels ID: {id}");
                        throw e;
                    }

                    ConsumptionInfo newConsumption = null;
                    try {
                        newConsumption = ApiDataFetcher.GetReelsConsumptionInfoFromJson(reelsJson["clips_metadata"]["original_sound_info"]["consumption_info"]);
                        DbWriter.WriteConsumptionInfo(newConsumption, dbContext, logger);
                    }catch (Exception) {
                        Logger.Error($"Could not get ConsumptionInfo data. Reels ID: {id}");
                        //throw e;
                    }

                    Friction newFriction = null;
                    try {
                        newFriction = ApiDataFetcher.GetReelsFrictionFromJson(reelsJson["sharing_friction_info"], id);
                        DbWriter.WriteFriction(newFriction, dbContext, logger);
                    }catch (Exception e) {
                        Logger.Error($"Could not get Friction Info data. Reels ID: {id}");
                        throw e;
                    }

                    MashupInfo newMashupInfo = null;
                    try {
                        newMashupInfo = ApiDataFetcher.GetReelsMashupInfoFromJson(reelsJson["clips_metadata"]["mashup_info"], newClipsMeta.Id);
                        DbWriter.WriteMashupInfo(newMashupInfo, dbContext, logger);
                    }catch (Exception e) {
                        Logger.Error($"Could not get Mashup Info Info data. Reels ID: {id}");
                        throw e;
                    }

                    OriginalSound newSound = null;
                    try {
                        newSound = ApiDataFetcher.GetReelsOriginalSoundFromJson(reelsJson["clips_metadata"]["original_sound_info"], newUser.Pk);
                        DbWriter.WriteOriginalSound(newSound, dbContext, logger);
                    }catch (Exception) {
                        Logger.Error($"Could not get Original Sound data. Reels ID: {id}");
                        //throw e;
                    }

                    ReelStats newReelStats = null;
                    try {
                        newReelStats = ApiDataFetcher.GetReelsStatsFromJson(reelsJson, newUser.Pk);
                        DbWriter.WriteReelStats(newReelStats, dbContext, logger);
                    }catch (Exception e) {
                        Logger.Error($"Could not get Reel Stats data. Reels ID: {id}");
                        throw e;
                    }

                    SquareCrop newCropInfo = null;
                    try {
                        newCropInfo = ApiDataFetcher.GetReelsCropInfoFromJson(reelsJson["media_cropping_info"]["square_crop"], id);
                        DbWriter.WriteSquareCrop(newCropInfo, dbContext, logger);
                    }catch (Exception e) {
                        Logger.Error($"Could not get Crop Info data. Reels ID: {id}");
                        throw e;
                    }

                    var newVideoVersions = new List<VideoVersion>();
                    try {
                        foreach (var videoVersion in reelsJson["video_versions"]) {
                            newVideoVersions.Add(ApiDataFetcher.GetReelsVideoVersionFromJson(videoVersion, id));
                        }
                        DbWriter.WriteVideoVersion(newVideoVersions, dbContext, logger);
                    }catch (Exception e) {
                        Logger.Error($"Could not get Video Version data.");
                        throw e;
                    }
                }
            }
        }
    }
}
