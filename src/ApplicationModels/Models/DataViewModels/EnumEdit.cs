namespace ApplicationModels.Models.DataViewModels {

    public enum EditType {
        New = 1,
        Update = 2,
        Delete = 3
    }

    // Type used to query the archive flag
    public enum ArchiveMode {
        UnArchived = 0,
        Archived = 1,
        All = 2
    }

    public enum PublishedMode {
        All,
        AllPublished,
        AllUnpublished,
        SomePublished
    }

    public enum AddOrRemove {
        Add = 1,
        Remove = 2
    }
}
