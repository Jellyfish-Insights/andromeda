using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationModels.Models.Metadata;
using Newtonsoft.Json.Linq;
namespace ApplicationModels.Models {
    public class ApplicationPlaylistApplicationVideo {
        public int ApplicationPlaylistId { get; set; }
        public ApplicationPlaylist ApplicationPlaylist { get; set; }
        public int ApplicationVideoId { get; set; }
        public ApplicationVideo ApplicationVideo { get; set; }
    }
}
