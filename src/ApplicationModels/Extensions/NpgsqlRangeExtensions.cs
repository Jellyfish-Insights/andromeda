using System;
using NpgsqlTypes;
namespace ApplicationModels.Extensions {
    public static class NpgsqlRangeExtensions {
        public static bool CheckUpper<T>(NpgsqlRange<T> range, T point) where T : IComparable<T> {
            if (range.UpperBoundInfinite) {
                return true;
            } else {
                var comp = range.UpperBound.CompareTo(point);
                if (range.UpperBoundIsInclusive)
                    return comp >= 0;
                else
                    return comp > 0;
            }
        }

        public static bool CheckLower<T>(NpgsqlRange<T> range, T point) where T : IComparable<T> {
            var comp = point.CompareTo(range.LowerBound);
            if (range.LowerBoundInfinite) {
                return true;
            } else {
                if (range.LowerBoundIsInclusive)
                    return comp >= 0;
                else
                    return comp > 0;
            }
        }

        public static bool Includes<T>(this NpgsqlRange<T> range, T point) where T : IComparable<T> {
            return CheckLower(range, point) && CheckUpper(range, point);
        }
    }
}
