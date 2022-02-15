using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DataLakeModels.Models.YouTube.Studio {

    public class Item : IEquatable<Item> {
        public Item() {}

        public string ItemId { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public string GroupId { get; set; }
        public Group Group { get; set; }

        bool IEquatable<Item>.Equals(Item other) {
            return ItemId == other.ItemId;
        }

        public override string ToString() {
            var belongsTo = GroupId == null ? "no group" : GroupId;
            return $"{ItemId} (belongs to {belongsTo})";
        }
    }
}
