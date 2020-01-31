using System.Collections.Generic;

namespace Jobs.Fetcher.Facebook {
    public interface PostgresTable {

        string TableName { get; }
        Dictionary<string, Column> ColumnDefinition { get; }
    }
}
