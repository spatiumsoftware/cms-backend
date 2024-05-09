using AutoMapper;
using Domain.ApplicationUserAggregate;
using Domain.Base;
using Domain.Interfaces;
using Domain.SubscriptionAggregate;
using Domain.SubscriptionAggregate.Input;
using Domian.Interfaces;
using Infrastructure.Services.PaymentWithPayal;
using Infrastructure.Services.PaymentWithStripeService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Spatium_CMS.Controllers.Blog;
using Spatium_CMS.Controllers.PaymentController.Request;
using Spatium_CMS.Controllers.SubscriptionController.Response;
using Utilities.Exceptions;
using Utilities.Results;
using Spatium_CMS.Controllers.PaymentController.Response;
namespace Spatium_CMS.Controllers.PaymentController
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : CmsControllerBase
    {
        private readonly IPaymentWithStripeService _paymentWithStripeService;
        private readonly ISendMailService sendMailService;
        private readonly PayPalSetting payPalSetting;

        public PaymentController(ILogger<BlogController> logger, IUnitOfWork unitOfWork, IMapper mapper,
            UserManager<ApplicationUser> userManager, IPaymentWithStripeService paymentWithStripeService ,
            ISendMailService sendMailService , PayPalSetting payPalSetting)
            : base(unitOfWork, mapper, logger, userManager)
        {
            _paymentWithStripeService = paymentWithStripeService;
            this.sendMailService = sendMailService;
            this.payPalSetting = payPalSetting;
        }


        // paypal 
        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        [Route("PaypalPayment")]
        public  Task<IActionResult> PaypalPayment(int subscriptionId)
        {



            return TryCatchLogAsync(async () =>
            {
                var blogId = GetBlogId();
                var allowPayment = await unitOfWork.SubscriptionRepository.CheckAllowSubscription(blogId, subscriptionId);
                if (allowPayment)
                {
                    if (subscriptionId == 1)
                    {
                        var blog = await unitOfWork.BlogRepository.GetByIdAsync(blogId);
                        blog.SwitchSubscription(subscriptionId);
                        await unitOfWork.SaveChangesAsync();
                        return Ok(new SpatiumResponse()
                        {
                            Success = true,
                            Message = "Switched To Free Plan Successfully "
                        });
                    }
                    var subscription = await unitOfWork.SubscriptionRepository.GetSubscriptionByIdAsync(subscriptionId) ?? throw new SpatiumException("there are no subscription with this id");
                //    var sessionRsponse = await _paymentWithStripeService.CreateCheckoutSession(blogId, (long)subscription.Price, subscription.Title, "http://localhost:4200/payment/success", "http://localhost:4200/cms/setting");
                    var paypalclient = new
                 PaypalClient(payPalSetting.ClientId, payPalSetting.ClientSecret, payPalSetting.Mode);
                       var order = await paypalclient.CreateOrder(subscription.Price.ToString(), "USD", Guid.NewGuid().ToString());


                    return Ok(order);
                }
                return BadRequest("You can not allowed to paid before fixed the problems of your current plan!!");

            });
        }
        [HttpGet]
        [Route("CaptureOrder")]
        public async Task<IActionResult> CaptureOrder(string orderId)
        {
            var paypalclient = new PaypalClient(payPalSetting.ClientId, payPalSetting.ClientSecret, payPalSetting.Mode);
            var response = await paypalclient.CaptureOrder(orderId);

            return Ok(response);
        }
        // paypal


        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        [Route("CreateCheckoutSession")]
        public Task<IActionResult> CreateCheckoutSession(int subscriptionId)
        {
            return TryCatchLogAsync(async () =>
            {
                var blogId=GetBlogId();

                var allowPayment =await unitOfWork.SubscriptionRepository.CheckAllowSubscription(blogId, subscriptionId);
                if (allowPayment)
                {
                    
                    if(subscriptionId ==1)
                    {
                        var blog = await unitOfWork.BlogRepository.GetByIdAsync(blogId);
                        blog.SwitchSubscription(subscriptionId);
                        await unitOfWork.SaveChangesAsync();
                        return Ok(new SpatiumResponse()
                        {
                            Success = true,
                            Message = "Switched To Free Plan Successfully "
                        });
                    }
                    var subscription = await unitOfWork.SubscriptionRepository.GetSubscriptionByIdAsync(subscriptionId) ?? throw new SpatiumException("there are no subscription with this id");
                    var sessionRsponse = await _paymentWithStripeService.CreateCheckoutSession(blogId,(long)subscription.Price, subscription.Title, "http://localhost:4200/payment/success", "http://localhost:4200/cms/setting");
                    return Ok(sessionRsponse);
                }
                return BadRequest("You can not allowed to paid before fixed the problems of your current plan!!");

            });
        }


        [HttpPost]
        [Authorize(Roles = "Super Admin")]
        [Route("PaymentConfirmation")]
        public Task<IActionResult> PaymentConfirmation(PaymentConfirmationRequest request)
        {
            return TryCatchLogAsync(async () =>
            {
                var subscription = await unitOfWork.SubscriptionRepository.GetSubscriptionByIdAsync(request.SubscriptionId) ?? throw new SpatiumException("Invalid Subscription Id ");
                // var respose = await _paymentWithStripeService.GetCheckoutSession(sessionId);
                var paymentType = await unitOfWork.SubscriptionRepository.GetPaymentTypeAsync((int)request.PaymentType)?? throw new SpatiumException("Invalid Payment Type Id ");
                var userId = GetUserId();
                var blogId = GetBlogId();
                var user = await userManager.FindByIdAsync(userId);
                var blog = await unitOfWork.BlogRepository.GetByIdAsync(blogId);
                blog.SwitchSubscription(request.SubscriptionId);
                await unitOfWork.SaveChangesAsync();
                var billinput = new BillHistoryInput()
                {
                    Email =user.Email,
                    Name = user.FullName,
                    Ammount = subscription.Price,
                    PaymentStatus = true ,
                    CreatedById = userId,
                    BlogId = blogId,
                    SubscriptionId = request.SubscriptionId,
                    Description = subscription.Title,
                    Currency = "usd",
                    PaymentTypeId = (int)request.PaymentType
                };
                var billhisorty = new BillingHistory(billinput);
                await  unitOfWork.SubscriptionRepository.InsertIntoHistory(billhisorty);
                await unitOfWork.SaveChangesAsync();
                Log.Information("user {name} paid {price} for subscribed in {subscription}", user.FullName, subscription.Price, subscription.Title);
                return Ok(new SpatiumResponse()
                {
                    Success = true,
                    Message = "Payment Successfully"
                });
              

            });
        }


        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        [Route("GetPaymentHistory")]
        public Task<IActionResult> GetPaymentHistory([FromQuery]GetEntityWithRange prams)
        {
            return TryCatchLogAsync(async () =>
            {
                var blogId = GetBlogId();

                if ((prams.StartDate != null && prams.EndDate == null) || (prams.EndDate != null && prams.StartDate == null)) throw new SpatiumException("You should enter both of date start and end date");

                if (prams.StartDate > prams.EndDate || prams.EndDate < prams.StartDate)
                    throw new SpatiumException("The datetime invalid !!");

                var billHistory = await unitOfWork.SubscriptionRepository.GetAllBillingHistory(prams, blogId);
               
                var totalItem =await unitOfWork.SubscriptionRepository.GetAllBillingHistoryAsync(blogId);
            
                var response = mapper.Map<IEnumerable<BillHistoryResponse>>(billHistory);

                return Ok(new
                {
                    Success = true,
                    Message = "Get Bill History",
                    Data = response,
                    Total=totalItem.Count(),
                    PagesNumber= totalItem.Count()/prams.PageSize+1,
                });

            });
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        [Route("FaildPaymentSendMail")]
        public Task<IActionResult> FaildPaymentSendMail()
        {
            return TryCatchLogAsync(async () =>
            {
                var userid= GetUserId();
                var user = await userManager.FindByIdAsync(userid);

                await sendMailService.SendMail(user.Email, "Spatium CMS", "Unfortunately ! your payment has failed, please complete the payment process to get the features on our site CMS Team");

                return Ok(new SpatiumResponse()
                {
                    Success = true,
                    Message = "Check Your Mail!"
                });

            });
        }


        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        [Route("GetInvoiceById")]
        public Task<IActionResult> GetInvoiceById(int Id)
        {
            return TryCatchLogAsync(async () =>
            {
                var billHistory = await unitOfWork.SubscriptionRepository.GetAllBillingHistoryByIdAsync(Id) ?? throw new SpatiumException("Invalid Id for billHistory");
                var blog = await unitOfWork.BlogRepository.GetByIdAsync(GetBlogId());

                var subscription=await unitOfWork.SubscriptionRepository.GetSubscriptionByIdAsync(billHistory.SubscriptionId);

                var response = new InvoiceResponse();
                response.Id = billHistory.Id;
                response.Name = billHistory.Name;
                response.Description = billHistory.Description;
                response.PaymentStatus = billHistory.PaymentStatus;
                response.Ammount=billHistory.Ammount;
                response.Email = billHistory.Email;
                response.Currency = billHistory.Currency;
                response.ExpirationDate=blog.SubscriptionAt.Date.AddMonths(1);
                response.NumberOfPosts=subscription.NumberOfPosts;
                response.NumberOfUsers=subscription.NumberOfUsers;
                response.SEO_Usage=subscription.SEO_Usage?"Allowed":"NotAllowed";
                response.StorageCapacity=subscription.StorageCapacity;
                response.PaymentType=billHistory.PaymentType.Name;
                return Ok(new
                {
                    Success = true,
                    Message = "Get BillHistory Succeded ",
                    data=response
                });
            });
        }
    

    }
}
