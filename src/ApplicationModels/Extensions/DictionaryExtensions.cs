using System;
using System.Collections.Generic;

namespace ApplicationModels.Extensions {
    public static class DictionaryExtensions {
        public static void AddOrUpdate<K, V>(this Dictionary<K, V> dictionary, K index, V newValue, Func<V, V> update) {
            try {
                // Some of the values stored might not be references, so I really need to access it twice
                dictionary[index] = update(dictionary[index]);
            } catch (KeyNotFoundException) {
                dictionary.Add(index, newValue);
            }
        }
    }
}
