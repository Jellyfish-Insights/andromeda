using System;
using System.Linq;
using ApplicationModels;

namespace ConsoleApp.Commands {

    public static class ArchiveVideosCommand {

        public static void ArchiveVideosPublishedBefore(DateTime date) {
            using (var context = new ApplicationDbContext()) {
                foreach (var video in context.ApplicationVideos.Where(x => !x.Archived)) {

                    var maxPublished = (from asr in context.ApplicationVideoSourceVideos
                                        where asr.ApplicationVideoId == video.Id
                                        join sv in context.SourceVideos on asr.SourceVideoId equals sv.Id
                                        select sv.PublishedAt).Max();

                    if (maxPublished != null && maxPublished < date) {
                        video.Archived = true;
                        video.UpdateDate = DateTime.UtcNow;
                    }
                }
                context.SaveChanges();
            }
        }
    }
}
