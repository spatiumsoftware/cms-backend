namespace Spatium_CMS.Controllers.PostController.Response
{
    public class ExternalPostResponse
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Content { get; set; }

        public string Category { get; set; }
        public string Tag { get; set; }
        public CreatedByResponse CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
