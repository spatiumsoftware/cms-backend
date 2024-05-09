using AutoMapper;
using Domain.ApplicationUserAggregate;
using Domian.Interfaces;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Spatium_CMS.Controllers.SubscriptionController.Response;
using Utilities.Exceptions;
using Utilities.Results;
namespace Spatium_CMS.Controllers.SubscriptionController
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : CmsControllerBase
    {


        public SubscriptionController(IUnitOfWork unitOfWork, IMapper maper,
      UserManager<ApplicationUser> userManager,ILogger<SubscriptionController> logger)
      : base(unitOfWork, maper, logger, userManager)
        {
         
        }

        [HttpGet]
        [Authorize(Roles ="Super Admin")]
        [Route("GetAllSubscription")]
        public Task<IActionResult> GetAllSubscription()
        {
            return TryCatchLogAsync(async () =>
            {
                var userId = GetUserId();
                var blogId= GetBlogId();

                var user = await userManager.FindUserInBlogAsync(blogId, userId) ?? throw new SpatiumException("You Are Not Allow");
                var blog = await unitOfWork.BlogRepository.GetByIdAsync(blogId)?? throw new SpatiumException("Invalid Blog Id");

                var subscriptions = await unitOfWork.SubscriptionRepository.GetAllSubscriptionAsync();
                var result = mapper.Map<IEnumerable<GetAllSubscriptionDto>>(subscriptions);

                foreach (var subscription in result) 
                {
                    if(subscription.Id == blog.SubscriptionId)
                        subscription.IsCurrentPlan = true;
                }

                //return Ok(new SpatiumResponse<IEnumerable<GetAllSubscriptionDto>>()
                //{
                //    Message = "Get All Subscription",
                //    Success = true ,
                //    Data = result
                //});
                return Ok(result);
            });
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        [Route("GetYourPlan")]
        public Task<IActionResult> GetYourPlan()
        {
            return TryCatchLogAsync(async () =>
            {
                var userId = GetUserId();
                var blogId = GetBlogId();

                var user = await userManager.FindUserInBlogAsync(blogId, userId) ?? throw new SpatiumException("You Are Not Allow");
                var blog = await unitOfWork.BlogRepository.GetByIdAsync(blogId) ?? throw new SpatiumException("Invalid Blog Id");
 
                var subscription = await unitOfWork.SubscriptionRepository.GetSubscriptionByBlogIdAsync(blogId);
                var result = mapper.Map<GetYourCurrentPlanDto>(subscription);
                result.AmmountOfUser = user.ApplicationUsers.Count();
                result.ExpDate = blog.SubscriptionAt.AddDays(subscription.Duration * 30);
                result.CountOfPost = blog.Posts.Count();
                result.CountOfCapacity =  unitOfWork.StorageRepository.GetBlogFilesCapcity(blogId);

                //return Ok(new SpatiumResponse<GetYourCurrentPlanDto>()
                //{
                //    Message = "Get Your Subscription",
                //    Success = true,
                //    Data = result
                //});
                return Ok(result);
            });


        }

        [HttpPut]
        [Authorize(Roles = "Super Admin")]
        [Route("SwitchOrSubscripPlan")]
        public Task<IActionResult> SwitchOrSubscripPlan(int newSubscriptionId)
        {
            return TryCatchLogAsync(async () =>
            {
                var blogId = GetBlogId();
                var blog = await unitOfWork.BlogRepository.GetByIdAsync(blogId);

                var newSubscription = await unitOfWork.SubscriptionRepository.GetSubscriptionByIdAsync(newSubscriptionId) ?? throw new SpatiumException("There Are No Subscription With This Id..");
                
                if(newSubscriptionId != 1)
                    throw  new SpatiumException("Go Payment First");
              
                var allowedCancel = await unitOfWork.SubscriptionRepository.CheckAllowSubscription(blogId, newSubscriptionId);
               
                if (allowedCancel)
                {
                    return Ok(new SpatiumResponse()
                    {
                        Message = $"you must go to paid",
                        Success = true,
                    });
                    //blog.SwitchSubscription(newSubscriptionId);
                    //await unitOfWork.SaveChangesAsync();
                };

                return Ok(new SpatiumResponse()
                {
                    Message = $"you can not subscribe or switch",
                    Success = false,
                });


            });
        }

        [HttpPut]
        [Authorize(Roles = "Super Admin")]
        [Route("CancelSubscriptionPlan")]
        public Task<IActionResult> CancelSubscriptionPlan()
        {
            return TryCatchLogAsync(async () =>
            {
                var blogId = GetBlogId();
                var blog = await unitOfWork.BlogRepository.GetByIdAsync(blogId);
                var defualtSubscriptionPlan = await unitOfWork.SubscriptionRepository.GetDefaultSubscriptionAsync();
                if (blog.SubscriptionId == defualtSubscriptionPlan.Id) throw new SpatiumException("You Are Already subscribed in Default Plan");
                var allowedCancel =await unitOfWork.SubscriptionRepository.CheckAllowSubscription(blogId, defualtSubscriptionPlan.Id);
                if (allowedCancel) {
                    blog.SwitchSubscription(defualtSubscriptionPlan.Id);
                    await unitOfWork.SaveChangesAsync();
                };
                return Ok(new SpatiumResponse()
                {
                    Message = $"Canceled Successfully",
                    Success = true,
                });
            });
        }


        #region Auto Renew
        //[HttpPost]
        //[Authorize(Roles = "Super Admin")]
        //[Route("AutoRenew")]
        //public Task <IActionResult> AutoRenew()
        //{
        //    return TryCatchLogAsync(async () =>
        //    {
        //        var blogId=GetBlogId();
        //        await _paymentService.CreateOrUpdateSubscriptionPlan(blogId);
        //        return Ok(new SpatiumResponse
        //        {
        //            Message="Auto Renew Succeded",
        //            Success = true,
        //        });
        //    });
        //}

        //[HttpPost]
        //[Authorize(Roles = "Super Admin")]
        //[Route("TurnOffAutoRenew")]
        //public Task<IActionResult> TurnOffAutoRenew()
        //{
        //    return TryCatchLogAsync(async () =>
        //    {
        //        var blogId = GetBlogId();
        //        var blog=await unitOfWork.BlogRepository.GetByIdAsync(blogId);
        //        if (!blog.IsRenew) throw new SpatiumException("You Are Already Turn Off Auto Renew");

        //        await _paymentService.CancelSubscriptionPlan(blogId);
        //        return Ok(new SpatiumResponse
        //        {
        //            Message = "Turn Of Auto Renew Succeded",
        //            Success = true,
        //        });
        //    });
        //} 
        #endregion 






    }
}
