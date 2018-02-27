using System;
using System.ComponentModel.DataAnnotations;

namespace DataLakeModels.Models.YouTube.Analytics {

    /**
        This is a helper model.

        It stores the most recent date for which we tried to get the
        "ViewerPercentage" metric for a given video.

        The intention is to save API quota, by avoiding redoing the
        same queries over and over.
     */
    public class ViewerPercentageLastDate {

        [Key]
        public string VideoId { get; set; }
        public DateTime Date { get; set; }
    }
}
