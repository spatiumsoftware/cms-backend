namespace Spatium_CMS.Controllers.SubscriptionController.Response
{
    public class BillHistoryResponse
    {
        public int Id { get; set; }
        public string Ammount { get;  set; }
        public string Currency { get;  set; }
        public string Description { get;  set; }
        public string Email { get;  set; }
        public string Name { get;  set; }
        public bool PaymentStatus { get;  set; }
        public DateTime ExpirationDate { get;  set; }
        public string PaymentType { get; set; }
    }
}
