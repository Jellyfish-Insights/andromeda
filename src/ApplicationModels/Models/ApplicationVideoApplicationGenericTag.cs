namespace ApplicationModels.Models {
    public class ApplicationVideoApplicationGenericTag {
        public int TagId { get; set; }
        public ApplicationGenericTag Tag { get; set; }
        public int VideoId { get; set; }
        public ApplicationVideo Video { get; set; }
    }
}
