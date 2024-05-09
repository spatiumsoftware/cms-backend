namespace Spatium_CMS.Controllers.SubscriptionController.Response
{
    public class GetYourCurrentPlanDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public decimal Price { get; set; }
        public int Duration { get; set; }
        public int AmmountOfUser { get; set; }
        public int NumberOfUsers { get;  set; }
        public bool SEOUsage { get;  set; }
        public int CountOfPost {  get; set; }
        public int? NumberOfPosts { get; set; }
        public double CountOfCapacity { get; set; }

        public int? StorageCapacity { get; set; }
        public DateTime ExpDate { get; set; }

        public bool IsCurrentPlan { get; set; } = true;
    }
}
