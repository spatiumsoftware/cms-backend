using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.PaymentWithPayal
{
    public class PayPalSetting
    {
        public string Mode { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
