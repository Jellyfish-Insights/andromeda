using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using DataLakeModels.Models.YouTube.Studio;
using Jobs.Fetcher.YouTubeStudio.Helpers;

namespace Jobs.Fetcher.YouTubeStudio
{
    sealed class YouTubeStudioFetchers
    {
        public static void Main(string[] args)
        {
            const string pathToData = @"../../../Data/";
            var videos = ImportFromFileSystem.GetVideosFromPath(pathToData);
            Console.WriteLine($"Decoded {videos.Count()} videos.");
        }
    }
}
