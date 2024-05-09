using Domain.ApplicationUserAggregate;
using Domain.Base;
using Domain.BlogsAggregate.Input;
using Domain.SubscriptionAggregate;

namespace Domain.BlogsAggregate
{
    public class Blog:EntityBase
    {
        #region CTOR
        public Blog()
        {

        }

        public Blog(BlogInput blogInput)
        {
            Name = blogInput.Name;
            FavIconPath = blogInput.FavIconPath;
            SubscriptionId = blogInput.SubscriptionId;
            IsRenew = false;
            SubscriptionAt = DateTime.UtcNow;
        }
        #endregion

        #region Properties
        public string Name { get; private set; }
        public string FavIconPath { get; private set; }
        public int SubscriptionId { get; private set; }
        public bool IsRenew { get; private set; }
        public DateTime SubscriptionAt { get; private set; }

        public string PaymentCustomerId { get; private set; }
        public string PaymentSubscriptionPlanId { get;private set; }

        #endregion

        #region Navigational Properties
        public virtual Subscription Subscription { get; private set; }
        #endregion

        #region Virtual Lists
        private readonly List<Post> _posts = new();
        public virtual IReadOnlyList<Post> Posts=> _posts.ToList();

        private readonly List<ApplicationUser> _users = new();
        public virtual IReadOnlyList<ApplicationUser> Users => _users.ToList();
        #endregion

        public void SwitchSubscription(int newSubscriptionId)
        {
            this.SubscriptionId = newSubscriptionId;
            this.SubscriptionAt = DateTime.UtcNow;
        }

        public void AddOrUpdateCustomerId(string customerId)
        {
            this.PaymentCustomerId = customerId;
        }
        public void AddOrUpdatePaymentSubscriptionPlanId(string SubscriptionPlanId)
        {
            this.PaymentSubscriptionPlanId = SubscriptionPlanId;
        }
        public void RenewSubscriptionPlan(bool isRenew)
        {
            this.IsRenew = isRenew;
        }
    }
}
