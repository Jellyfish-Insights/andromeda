using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationModels.Models.Metadata;
using Newtonsoft.Json.Linq;
using TypeScriptBuilder;

namespace ApplicationModels.Models {
    public class SourcePlaylist : IMutableEntity {
        [Key]
        public string Id { get; set; }
        public string Platform { get; set; }
        public string Name { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Description { get; set; }
        public DateTime UpdateDate { get; set; }
        public List<SourcePlaylistSourceVideo> PlaylistVideos { get; set; }

        [NotMapped]
        [TSExclude]
        public List<JToken> PrimaryKey { get => MutableEntityExtentions.AutoPK(Id); }
    }
}
