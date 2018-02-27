using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationModels.Models.Metadata;
using Newtonsoft.Json.Linq;
using TypeScriptBuilder;

namespace ApplicationModels.Models {
    public class SourcePlaylistSourceVideo : IMutableEntity {
        public string VideoId { get; set; }
        public SourceVideo Video { get; set; }
        public string PlaylistId { get; set; }
        public SourcePlaylist Playlist { get; set; }
        public DateTime UpdateDate { get; set; }

        [NotMapped]
        [TSExclude]
        public List<JToken> PrimaryKey { get => MutableEntityExtentions.AutoPK(VideoId, PlaylistId); }
    }
}
