using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using DataLakeModels.Models.YouTube.Studio;
using Jobs.Fetcher.YouTubeStudio.Helpers;
using Andromeda.Common.Jobs;

namespace Jobs.Fetcher.YouTubeStudio
{
    sealed class YouTubeStudioFetchers : FetcherJobsFactory
    {
        public override JobScope Scope {get;} = JobScope.YouTubeStudio;
        public override IEnumerable<AbstractJob> GetJobs(
            JobType type, JobScope scope, IEnumerable<string> names, JobConfiguration config
        ){
            return null;
        }
        public static void Main(string[] args)
        {
            const string pathToData = @"../../../Data/";
            var videos = ImportFromFileSystem.GetVideosFromPath(pathToData);
            Console.WriteLine($"Decoded {videos.Count()} videos.");
        }
    }
}
