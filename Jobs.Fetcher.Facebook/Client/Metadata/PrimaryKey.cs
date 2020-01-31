using System.Linq;

namespace Jobs.Fetcher.Facebook {
    public class PrimaryKey {
        /**
           usage:
            - entity: {NominalColumns: (id), TemporalColumns: (), VersionColumn: systime}
            - entity's edge: {NominalColumns: (source_id, id), TemporalColumns: (), VersionColumn: systime}
            - entity's daily metric: {NominalColumns: (source_id), TemporalColumns: (date_start, date_end), VersionColumn: systime}
            - entity's lifetime metric: {NominalColumns: (source_id), TemporalColumns: (), VersionColumn: systime}

            - schema's edge: {NominalColumns: (id), TemporalColumns: (), VersionColumn: systime}
            - schemas's daily metric: {NominalColumns: (), TemporalColumns: (date_start, date_end), VersionColumn: systime}
            - schemas's lifetime metric: {NominalColumns: (), TemporalColumns: (), VersionColumn: systime}
         */
        public PrimaryKey(Column[] nominalColumns, bool useTemporalColumns) {
            NominalColumns = nominalColumns;
            TemporalColumns = (useTemporalColumns) ? Constants.DefaultTemporalColumns : Constants.NoTemporalColumns;
            VersionColumn = Constants.SystimeColumn;
        }

        public Column[] NominalColumns { get; }
        public Column[] TemporalColumns { get; }
        public Column VersionColumn { get; }
        public Column[] Columns {
            get { return NominalColumns.Concat(TemporalColumns).Append(VersionColumn).ToArray(); }
        }
        public string[] ColumnNames {
            get { return Columns.Select(x => x.Name).ToArray(); }
        }
        public string[] NominalColumnNames {
            get { return NominalColumns.Select(x => x.Name).ToArray(); }
        }
        public string[] TemporalColumnNames {
            get { return TemporalColumns.Select(x => x.Name).ToArray(); }
        }
    }
}
