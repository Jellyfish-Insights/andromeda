using System;
using System.Collections.Generic;
using System.Linq;
using Jobs.Fetcher.YouTubeStudio.Helpers;

namespace Jobs.Fetcher.YouTubeStudio {

    public class VideosQuery : YouTubeStudioFetcher {
        public VideosQuery(){}

        public override List<string> Dependencies() {
            return new List<string>() {};
        }

        override public void RunBody() {
        }
    }
}
