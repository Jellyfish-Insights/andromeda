namespace Jobs.Fetcher.Facebook {
    public class Metrics : Column {
        public Metrics(string[] args): base(args) {
            if (args.Length > 4) {
                Resolution = args[3];
            }
        }

        public string Resolution { get; set; }
    }
}
