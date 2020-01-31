namespace Jobs.Fetcher.Facebook {
    public class Constants {
        public static Column SystimeColumn = new Column("systime", "tsrange");
        // fetch_time is injected on the response of every API request
        public static Column FetchTime = new Column("fetch_time", "timestamp without time zone");
        public static Column DateStartColumn = new Column("date_start", "timestamp without time zone");
        public static Column DateStopColumn = new Column("date_stop", "timestamp without time zone");
        public static Column[] NoNominalColumn = new Column[] {};
        public static Column[] NoTemporalColumns = new Column[] {};
        public static Column[] DefaultTemporalColumns = new Column[] { DateStartColumn, DateStopColumn };
    }
}
