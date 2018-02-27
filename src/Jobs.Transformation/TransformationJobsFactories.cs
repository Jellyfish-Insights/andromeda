using System.Collections.Generic;
using Common.Jobs;

namespace Jobs.Transformation {

    public class YouTubeTransformations : TransformationJobsFactory {
        public override JobScope Scope { get; } = JobScope.YouTube;

        public override IEnumerable<AbstractJob> GetJobs(JobType type, JobScope scope, IEnumerable<string> names, JobConfiguration jobConfiguration) {
            if (CheckTypeAndScope(type, scope)) {
                return NoJobs;
            }

            var jobs = new List<AbstractJob>() {
                new YouTube.VideoSync(),
                new YouTube.PlaylistSync(),
                new YouTube.PlaylistVideoSync(),
                new YouTube.VideoMetricSync(),
                new YouTube.VideoDemographicMetricSync(),
                new YouTube.StatisticsSync(),
            };

            return FilterByName(jobs, names);
        }
    }

    public class AdWordsTransformations : TransformationJobsFactory {
        public override JobScope Scope { get; } = JobScope.AdWords;

        public override IEnumerable<AbstractJob> GetJobs(JobType type, JobScope scope, IEnumerable<string> names, JobConfiguration jobConfiguration) {
            if (CheckTypeAndScope(type, scope)) {
                return NoJobs;
            }

            var jobs = new List<AbstractJob>() {
                new AdWords.CampaignSync(),
                new AdWords.AudienceSync(),
                new AdWords.AdSetSync(),
                new AdWords.AdSync(),
                new AdWords.AdMetricSync(),
            };

            return FilterByName(jobs, names);
        }
    }

    public class FacebookTransformations : TransformationJobsFactory {
        public override JobScope Scope { get; } = JobScope.Facebook;

        public override IEnumerable<AbstractJob> GetJobs(JobType type, JobScope scope, IEnumerable<string> names, JobConfiguration jobConfiguration) {
            if (CheckTypeAndScope(type, scope)) {
                return NoJobs;
            }

            var jobs = new List<AbstractJob>() {
                new Facebook.VideoSync(),
                new Facebook.VideoMetricSync(),
                new Facebook.DeltaEncodedVideoMetricSync(),
                new Facebook.VideoDemographicMetricSync(),
                new Facebook.PlaylistSync(),
                new Facebook.PlaylistVideoSync(),
                new Facebook.CampaignSync(),
                new Facebook.AdSync(),
                new Facebook.AdMetricSync(),
                new Facebook.AudienceSync(),
                new Facebook.FlexibleAudienceSync(),
                new Facebook.AdSetSync(),
            };

            return FilterByName(jobs, names);
        }
    }

    public class ApplicationTransformations : TransformationJobsFactory {
        public override JobScope Scope { get; } = JobScope.Application;

        public override IEnumerable<AbstractJob> GetJobs(JobType type, JobScope scope, IEnumerable<string> names, JobConfiguration jobConfiguration) {
            if (CheckTypeAndScope(type, scope)) {
                return NoJobs;
            }

            var jobs = new List<AbstractJob>() {
                new Application.UpsertVideoAssociations(),
                new Application.UpsertApplicationPlaylists(),
                new Application.UpsertPlaylistAssociations(),
                new Application.CreatePersonas(),
                new Application.CreatePersonaVersions(),
                new Application.UpsertPersonaVersionAdSetRelation(),
                new Application.AutomaticVideoLengthTagging(),
                new Application.UpsertCampaignAssociations()
            };

            return FilterByName(jobs, names);
        }
    }
}
