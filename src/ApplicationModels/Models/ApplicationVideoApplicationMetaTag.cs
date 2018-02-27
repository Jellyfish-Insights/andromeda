namespace ApplicationModels.Models {
    public class ApplicationVideoApplicationMetaTag {
        public int TypeId { get; set; }
        public ApplicationMetaTagType Type { get; set; }
        public int TagId { get; set; }
        public ApplicationMetaTag Tag { get; set; }
        public int VideoId { get; set; }
        public ApplicationVideo Video { get; set; }
    }
}
