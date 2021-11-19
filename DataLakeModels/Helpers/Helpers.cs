using System;
using System.Linq;
using System.Collections.Generic;
using DataLakeModels.Models;

namespace DataLakeModels.Helpers {

    public static class CompareEntries {

        public static Modified CompareOldAndNewEntry<T>(T oldEntry, T newEntry) where T : IEquatable<T> {
            if (oldEntry == null)
                return Modified.New;

            if (!oldEntry.Equals(newEntry))
                return Modified.Updated;

            return Modified.Equal;
        }

        public static Modified CompareOldAndNewEntries<T>(IEnumerable<T> oldEntries, IEnumerable<T> newEntries) where T : IEquatable<T> {
            if (oldEntries == null)
                return Modified.New;

            if (!oldEntries.ToHashSet().SetEquals(newEntries)) {
                return Modified.Updated;
            }
            return Modified.Equal;
        }
    }
}
