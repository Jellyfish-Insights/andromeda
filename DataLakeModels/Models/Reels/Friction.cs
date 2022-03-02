using System;
using System.Runtime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataLakeModels.Models.Reels {
    public class Friction : IEquatable<Friction> {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Reel Reel { get; set; }
        public string ReelId { get; set; }
        public string BloksAppUrl { get; set; }
        public long ShouldHaveSharingFriction { get; set; }

        bool IEquatable<Friction>.Equals(Friction other) {
            return Id == other.Id &&
                   BloksAppUrl == other.BloksAppUrl &&
                   ShouldHaveSharingFriction == other.ShouldHaveSharingFriction;
        }
    }
}
