using Domain.ApplicationUserAggregate;
using Domain.Base;
using Domain.BlogsAggregate;
using Domain.LookupsAggregate;
using Domain.SubscriptionAggregate.Input;

namespace Domain.SubscriptionAggregate
{
    public class BillingHistory :EntityBase
    {
        #region Prop 
        public decimal Ammount   { get;private set; }
        public string Currency { get;private set; }
        public string Description { get;private set; }
        public string  Email { get;private set; }
        public string Name { get;private set; }
        public bool PaymentStatus { get; private set; }
        public string CreatedById {  get; private set; }
        public int SubscriptionId { get;private set; }
        public int BlogId { get; private set; }
        public int PaymentTypeId {  get; private set; }

        #endregion
        #region Nav
        public virtual Blog Blog { get; private set; }
        public virtual ApplicationUser  CreatedBy { get; private set; }
        public virtual Subscription Subscription { get; private set; }
        public virtual PaymentType PaymentType { get; private set; }
        #endregion

        #region Ctro
        public BillingHistory()
        {
            
        }
        public BillingHistory(BillHistoryInput input)
        {
            this.IsDeleted = false;
            this.CreationDate = DateTime.UtcNow;
            this.Ammount = input.Ammount;
            this.Currency = input.Currency;
            this.Description = input.Description;
            this.Email = input.Email;
            this.Name = input.Name;
            this.PaymentStatus = input.PaymentStatus;
            this.CreatedById = input.CreatedById;
            this.SubscriptionId = input.SubscriptionId;
            this.BlogId = input.BlogId;
            this.PaymentTypeId = input.PaymentTypeId;
        }
        #endregion
    }
}
