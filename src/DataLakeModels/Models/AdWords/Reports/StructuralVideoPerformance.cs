using System;

namespace DataLakeModels.Models.AdWords.Reports {

    public class StructuralVideoPerformance : IValidityRange {

        public DateTime ValidityStart { get; set; }
        public DateTime ValidityEnd { get; set; }

        public string CreativeId { get; set; }
        public string VideoId { get; set; }
    }
}
