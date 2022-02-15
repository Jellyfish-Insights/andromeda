using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace DataLakeModels.Models.YouTube.Studio {

    public class Group : IEquatable<Group> {
        public Group() {}

        public string GroupId { get; set; }
        public string Title { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public List<Item> Items { get; set; }

        public override bool Equals(object obj) {
            if (!(obj is Group)) {
                return false;
            }
            return Equals(obj as Group);
        }

        bool IEquatable<Group>.Equals(Group other) {
            return GroupId == other.GroupId;
        }

        public override int GetHashCode() => GroupId.GetHashCode();

        public override string ToString() {
            var title = Title == null ? "<null>" : Title;
            var items = Items == null ? new List<Item>() : Items;
            return $@"
                GroupId: {GroupId},
                Title: {title},
                RegistrationDate: {RegistrationDate},
                UpdateDate: {UpdateDate},
                Items: {String.Join(", ", items.Select( it => it.ItemId ))},
                ItemCount: {items.Count()}
            ";
        }
    }
}
