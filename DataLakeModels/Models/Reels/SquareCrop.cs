using System;
using System.Runtime;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DataLakeModels.Models.Reels {
    public class SquareCrop : IEquatable<SquareCrop> {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Reel Reel { get; set; }
        public string ReelId { get; set; }
        public double Top { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public double Bottom { get; set; }

        bool IEquatable<SquareCrop>.Equals(SquareCrop other) {
            return Id == other.Id &&
                   Top == other.Top &&
                   Left == other.Left &&
                   Right == other.Right &&
                   Bottom == other.Bottom;
        }
    }
}
