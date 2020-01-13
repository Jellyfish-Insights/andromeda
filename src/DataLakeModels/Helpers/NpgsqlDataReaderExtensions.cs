using System;
using Npgsql;

namespace DataLakeModels.Helpers {

    public static class NpgsqlDataReaderExtensions {
        public static T Prim<T>(this NpgsqlDataReader reader, string name) {
            var ordinal = reader.GetOrdinal(name);
            if (reader.IsDBNull(ordinal)) {
                throw new NullReferenceException("accessing non nullable fied");
            } else {
                return (T) reader.GetFieldValue<T>(ordinal);
            }
        }

        public static T DefPrim<T>(this NpgsqlDataReader reader, string name) where T : struct {
            var ordinal = reader.GetOrdinal(name);
            if (reader.IsDBNull(ordinal)) {
                return default(T);
            } else {
                return reader.GetFieldValue<T>(ordinal);
            }
        }

        public static T? OptPrim<T>(this NpgsqlDataReader reader, string name) where T : struct {
            var ordinal = reader.GetOrdinal(name);
            if (reader.IsDBNull(ordinal)) {
                return null;
            } else {
                return (T?) reader.GetFieldValue<T>(ordinal);
            }
        }

        public static T OptClass<T>(this NpgsqlDataReader reader, string name) {
            var ordinal = reader.GetOrdinal(name);
            if (reader.IsDBNull(ordinal)) {
                return default(T);
            } else {
                return reader.GetFieldValue<T>(ordinal);
            }
        }
    }
}
