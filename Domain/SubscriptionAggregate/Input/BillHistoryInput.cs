using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.SubscriptionAggregate.Input
{
    public class BillHistoryInput
    {
        public decimal Ammount { get;  set; }
        public string Currency { get;  set; }
        public string Description { get;  set; }
        public string Email { get;  set; }
        public string Name { get;  set; }
        public bool PaymentStatus { get;  set; }
        public string CreatedById { get;  set; }
        public int SubscriptionId { get;  set; }
        public int BlogId { get;  set; }
        public int PaymentTypeId { get;  set; }

    }
}
