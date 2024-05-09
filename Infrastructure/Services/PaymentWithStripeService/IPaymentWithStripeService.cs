using Infrastructure.Services.PaymentWithStripeService.Models;
using Stripe.Checkout;
namespace Infrastructure.Services.PaymentWithStripeService
{
    public interface IPaymentWithStripeService
    {
        Task<SessisonResponse> CreateCheckoutSession(int blogId,long price, string description, string SuccessUrl, string CancelUrl);
        Task<Session> GetCheckoutSession(string sessionId);
        Task CreateOrUpdateSubscriptionPlan(int blogId);
        Task CancelSubscriptionPlan(int blogId);
    }
}
