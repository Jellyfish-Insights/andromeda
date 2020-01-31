namespace Jobs.Fetcher.Facebook {
    public class Column {
        public Column(string name, string type, string enforcedApiName = null) {
            Name = name;
            Type = type;
            EnforcedApiName = enforcedApiName;
        }

        public Column(string[] args) {
            Name = args[0];
            Type = args[1];
            if (args.Length > 2)
                Constraint = args[2];
            if (args.Length > 3)
                ApiAnnotation = args[3];
            if (args.Length > 4)
                EnforcedApiName = args[4];
        }

        // Column name on the relational database
        public string Name { get; set; }
        public string Type { get; set; }
        public string Constraint { get; set; }
        private string ApiAnnotation { get; set; }

        // Used for building the http url for this column
        public string ApiName() { return Name + ApiAnnotation; }

        private string EnforcedApiName;
        // When reading the response from the Api, this is how the data is accessed
        public string ApiResponseName {
            get {
                return EnforcedApiName ?? Name;
            }
        }
        // Unique identifier used for writing query expression with parameters
        public uint Hash() { return (uint) Name.GetHashCode(); }

        public Column Clone(string newTableName = null) {
            if (newTableName != null) {
                return new Column($"{newTableName}_{Name}", Type, Name);
            }
            return new Column(Name, Type);
        }
    }
}
