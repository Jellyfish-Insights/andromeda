using System;

namespace DataLakeModels.Models.YouTube.Analytics {

    public class ViewerPercentage : IValidityRange, IEquatable<ViewerPercentage> {

        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }

        /**
            These two values implement the "coverage" range. This is used to avoid
            storing repeated values for subsequent days on which the value has not
            changed.

            For instance:

            From date 0 to date 4, the value is x. From date 4 to date 10, the value
            is y. And so on.

            Difference from "Validity Range":

            On validity date 0, the value time series was: x - - - - y - - z - -   (each character represents a time point)

            On validity "now", the value time series is:   x - - y' - - - z' - t' - -

            So, at different points in time (validity range), the system had a different
            idea of what were the values in the time series (coverage range).
         */
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string VideoId { get; set; }
        public string Gender { get; set; }
        public string AgeGroup { get; set; }
        public double Value { get; set; }

        public bool Equals(ViewerPercentage other) {
            return Value == other.Value
                   && Gender == other.Gender
                   && AgeGroup == other.AgeGroup;
        }

        public override int GetHashCode() {
            return $"{Gender}_{AgeGroup}_{Value}".GetHashCode();
        }
    }
}
