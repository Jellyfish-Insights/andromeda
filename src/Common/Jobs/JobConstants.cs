namespace Common.Jobs {
    public static class JobConstants {
        public const string JobConfigFile = "job_config.json";
        public static JobConfiguration DefaultJobConfiguration = new JobConfiguration {
            IgnoreAPI = false,
            IgnoreEdges = false,
            MaxEntities = 0,
            Paginate = true,
        };
    }
}
