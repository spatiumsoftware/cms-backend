namespace Spatium_CMS.Controllers.PaymentController.Response
{
    public class InvoiceResponse
    {
        public int Id { get; set; }
        public decimal Ammount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public bool PaymentStatus { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string PaymentType { get; set; }
        public int NumberOfUsers { get;  set; }
        public string SEO_Usage { get; set;}
        public int? NumberOfPosts { get;  set; }
        public int? StorageCapacity { get;  set; }
    }
}
