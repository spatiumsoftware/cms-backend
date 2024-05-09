

using Domain.Base;
using Domain.LookupsAggregate;
using System.Runtime.InteropServices;

namespace Domain.SubscriptionAggregate
{
    public interface ISubscriptionRepository
    {
        Task<Subscription> GetDefaultSubscriptionAsync();
        Task<Subscription> GetSubscriptionByIdAsync(int subscriptionId);
        Task<IEnumerable<Subscription>> GetAllSubscriptionAsync();
        Task<Subscription> GetSubscriptionByBlogIdAsync(int blogId);
        Task<bool> CheckAllowSubscription(int blogId,int subscriptionId);
        Task<IEnumerable<BillingHistory>> GetAllBillingHistory(GetEntityWithRange entitiyParams, int blogId);
        Task<IEnumerable<BillingHistory>> GetAllBillingHistoryAsync(int BlogId);
        Task InsertIntoHistory(BillingHistory history);
        Task<PaymentType> GetPaymentTypeAsync(int id);
        Task<BillingHistory> GetAllBillingHistoryByIdAsync(int Id);
    }
}
