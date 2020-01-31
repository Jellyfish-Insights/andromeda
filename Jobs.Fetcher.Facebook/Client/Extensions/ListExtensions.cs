using System.Collections.Generic;

namespace Jobs.Fetcher.Facebook {

    static class ListExtensions {

        public static T Pop<T>(this List<T> list, int index) {
            var value = list[index];
            list.RemoveAt(index);
            return value;
        }
    }
}
