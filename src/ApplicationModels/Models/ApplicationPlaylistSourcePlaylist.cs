using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationModels.Models.Metadata;
using Newtonsoft.Json.Linq;
namespace ApplicationModels.Models {
    public abstract class ApplicationPlaylistSourcePlaylist : IMutableEntity {
        public int ApplicationPlaylistId { get; set; }
        public ApplicationPlaylist ApplicationPlaylist { get; set; }
        public string SourcePlaylistId { get; set; }
        public DateTime UpdateDate { get; set; }

        [NotMapped]
        public List<JToken> PrimaryKey { get => MutableEntityExtentions.AutoPK(ApplicationPlaylistId, SourcePlaylistId); }
    }

    public class GeneratedApplicationPlaylistSourcePlaylist : ApplicationPlaylistSourcePlaylist {}
}
