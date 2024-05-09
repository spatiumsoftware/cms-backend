
using Domain.ApplicationUserAggregate;
using Domian.Interfaces;
using Infrastructure.Services.PaymentWithStripeService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace Infrastructure.Services.PaymentWithStripeService
{
    public class PaymentWithStripeService : IPaymentWithStripeService
    {
        private readonly IConfiguration configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        public PaymentWithStripeService(IConfiguration configuration,IUnitOfWork unitOfWork,UserManager<ApplicationUser> userManager)
        {
            this.configuration = configuration;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<SessisonResponse> CreateCheckoutSession(int blogId,long price,string description,string SuccessUrl,string CancelUrl)
        {

            StripeConfiguration.ApiKey = configuration["StripeSettings:SecretKey"];

            var blog = await _unitOfWork.BlogRepository.GetByIdAsync(blogId);
            var customerId = "";

            if (blog.PaymentCustomerId == null)
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.BlogId == blog.Id);
                var CustomerOptions = new CustomerCreateOptions
                {
                    Name = user.FullName,
                    Email = user.Email,
                };
                var CustomerService = new CustomerService();
                var customer = await CustomerService.CreateAsync(CustomerOptions);

                customerId = customer.Id;
                blog.AddOrUpdateCustomerId(customerId);
            }
            else
            {
                customerId = blog.PaymentCustomerId;
            };
            await _unitOfWork.SaveChangesAsync();

            var options = new SessionCreateOptions
            {
                Customer=customerId,
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = description,
                                },
                                UnitAmount = price * 100,

                            },
                            Quantity = 1,
                            
                        },
                    },
                Mode = "payment",
                SuccessUrl = SuccessUrl,
                CancelUrl = CancelUrl,
                
                
            };

            var service = new SessionService();
            var session =await service.CreateAsync(options);
        

            return new SessisonResponse
            {
                SessionId=session.Id,
                SessionUrl=session.Url,
                SuccessUrl=session.SuccessUrl,
                CancelUrl = session.CancelUrl
            };

        }

        public async  Task<Session> GetCheckoutSession(string sessionId)
        {
            StripeConfiguration.ApiKey = configuration["StripeSettings:SecretKey"];
            var service = new SessionService();
            var resopnse =await service.GetAsync(sessionId);
            return resopnse;
        }

        public async Task CreateOrUpdateSubscriptionPlan(int blogId)
        {
            StripeConfiguration.ApiKey = configuration["StripeSettings:SecretKey"];
            var blog= await _unitOfWork.BlogRepository.GetByIdAsync(blogId);
            var subscriptionPlan=await _unitOfWork.SubscriptionRepository.GetSubscriptionByIdAsync(blog.SubscriptionId);
           

            var priceOptions = new PriceCreateOptions
            {
                Currency = "usd",
                UnitAmount = (long)subscriptionPlan.Price * 100,
                Recurring = new PriceRecurringOptions { 
                    Interval = "month",
                    IntervalCount = 1,
                },
                ProductData = new PriceProductDataOptions {
                    Name = subscriptionPlan.Title,
                    Metadata = new Dictionary<string, string>() {
                        {
                           "NumberOfUsers",subscriptionPlan.NumberOfUsers.ToString()
                        },
                         {
                           "NumberOfPosts",subscriptionPlan.NumberOfPosts.ToString()
                        },
                          {
                           "SEO_Usage ",subscriptionPlan.SEO_Usage?"Allowed":"NotAllowed"
                        }
                    }
                },
             
            };
            var PriceService = new PriceService();
            var price=await PriceService.CreateAsync(priceOptions);


            if (blog.PaymentSubscriptionPlanId == null) {

                var subscriptionOptions = new SubscriptionCreateOptions
                {
                    Customer = blog.PaymentCustomerId,
                
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions { Price = price.Id },
                    },
                    BackdateStartDate = blog.SubscriptionAt,
                    Metadata = new Dictionary<string, string>()
                    {
                        {
                            "NumberOfUsers",subscriptionPlan.NumberOfUsers.ToString()
                        },
                            {
                            "NumberOfPosts",subscriptionPlan.NumberOfPosts.ToString()
                        },
                        {
                            "SEO_Usage ",subscriptionPlan.SEO_Usage?"Allowed":"NotAllowed"
                        }
                    }

                };

                var subscriptionService = new SubscriptionService();
                var newSubscription =await subscriptionService.CreateAsync(subscriptionOptions);
                blog.AddOrUpdatePaymentSubscriptionPlanId(newSubscription.Id);
            }
            else
            {

                var options = new SubscriptionUpdateOptions
                {
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions { Price = price.Id },
                    },
                };
                var service = new SubscriptionService();
                await service.UpdateAsync(blog.PaymentSubscriptionPlanId, options);
            }


            blog.RenewSubscriptionPlan(true);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task CancelSubscriptionPlan(int blogId)
        {
            StripeConfiguration.ApiKey = configuration["StripeSettings:SecretKey"];

            var blog=await _unitOfWork.BlogRepository.GetByIdAsync(blogId);
            var subscriptionPlanId = blog.PaymentSubscriptionPlanId;

            var service = new SubscriptionService();
            blog.RenewSubscriptionPlan(false);
            await _unitOfWork.SaveChangesAsync();
            await service.CancelAsync(subscriptionPlanId);

        }
    }
}
