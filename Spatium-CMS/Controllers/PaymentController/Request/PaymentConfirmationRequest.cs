using Utilities.Enums;

namespace Spatium_CMS.Controllers.PaymentController.Request
{
    public class PaymentConfirmationRequest
    {
        public int SubscriptionId { get; set; }
        public int PaymentType { get; set; }
    }
}
