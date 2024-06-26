﻿using AutoMapper;
using Domain.ApplicationUserAggregate;
using Domain.ApplicationUserAggregate.Inputs;
using Domain.Base;
using Domain.BlogsAggregate;
using Domain.Interfaces;
using Domian.Interfaces;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Spatium_CMS.AttachmentService;
using Spatium_CMS.Controllers.AuthenticationController.Response;
using Spatium_CMS.Controllers.UserManagmentController.Request;
using Spatium_CMS.Controllers.UserManagmentController.Response;
using Spatium_CMS.Filters;
using System.Net;
using Utilities.Enums;
using Utilities.Exceptions;
using Utilities.Extensions;
using Utilities.Helpers;
using Utilities.Results;

namespace Spatium_CMS.Controllers.UserManagmentController
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserManagmentController : CmsControllerBase
    {
        private readonly ISendMailService sendMailService;
        private readonly IAttachmentService attachmentService;
        private readonly IConfiguration configration;
        private readonly IWebHostEnvironment enviroenment;
        private readonly IConfiguration configuration;

        public UserManagmentController(IUnitOfWork unitOfWork, IMapper maper,
            UserManager<ApplicationUser> userManager, ISendMailService sendMailService, ILogger<UserManagmentController> logger, IAttachmentService attachmentService, IConfiguration configration, IWebHostEnvironment enviroenment)
            : base(unitOfWork, maper, logger, userManager)
        {
            this.sendMailService = sendMailService;
            this.attachmentService = attachmentService;
            this.configration = configration;
            this.enviroenment = enviroenment;
        }

        [HttpPost]
        [Route("CreateUser")]
        [Authorize(Roles = "Super Admin")]
        [PermissionFilter(PermissionsEnum.CreateUser)]
        public Task<IActionResult> CreateUser(CreateUserRequest createUserRequest)
        {
            return TryCatchLogAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    if (createUserRequest.RoleId == MainRolesIdsEnum.SuperAdmin.GetDescription())
                        throw new SpatiumException(ResponseMessages.InvalidRole);
                    var userId = GetUserId();
                    var blogId = GetBlogId();

                    var blog = await unitOfWork.BlogRepository.GetByIdAsync(blogId);
                    var blogUsers = blog.Users.Count;

                    var subscription = await unitOfWork.SubscriptionRepository.GetSubscriptionByIdAsync(blog.SubscriptionId);
                    var subscriptionUsers = subscription.NumberOfUsers;
                    if (blogUsers - 1 >= subscriptionUsers && blog.SubscriptionId != 4) throw new SpatiumException("You reached the limit of the number for creating users!!..upgrade your subscription plan or delete user");

                    if (blogUsers - 1 >= subscriptionUsers && blog.SubscriptionId == 4) throw new SpatiumException("You reached the limit of the number for creating users!!");
                    var loginUser = await userManager.FindByIdAsync(userId) ?? throw new SpatiumException(ResponseMessages.UserNotFound);

                    if (createUserRequest.ImageProfile != null)
                    {
                        if (createUserRequest.ImageProfile.ContentType.ToLower() != "image/jpg" &&
                        createUserRequest.ImageProfile.ContentType.ToLower() != "image/x-png" &&
                        createUserRequest.ImageProfile.ContentType.ToLower() != "image/png")
                            throw new SpatiumException("The Image You Are Selected Or Uploaded Is Not Valid!!");
                    }

                    string filePath = createUserRequest.ImageProfile != null ? await attachmentService.SaveAttachment($"{blogId}/ImagesProfile/", createUserRequest.ImageProfile, enviroenment.WebRootPath, userId) : null;

                    var applicationUserInput = new ApplicationUserInput();
                    applicationUserInput.FullName = createUserRequest.FullName;
                    applicationUserInput.ProfileImagePath = filePath;
                    applicationUserInput.Email = createUserRequest.Email;
                    applicationUserInput.RoleId = createUserRequest.RoleId;
                    applicationUserInput.JobTitle = "UnAssigned";
                    applicationUserInput.ParentUserId = loginUser.Id;
                    applicationUserInput.ParentBlogId = loginUser.BlogId;
                    var applicationUser = new ApplicationUser(applicationUserInput);

                    var identityResult = await userManager.CreateAsync(applicationUser, createUserRequest.Password);

                    if (identityResult.Succeeded)
                    {
                        var token = await userManager.GenerateEmailConfirmationTokenAsync(applicationUser);
                        var encodedToken = WebUtility.UrlEncode(token);
                        applicationUser.ChangeOTP(OTPGenerator.GenerateOTP());
                        await unitOfWork.SaveChangesAsync();

                        string MailBody = $"<h1> Your OTP is {applicationUser.OTP} </h1> <br/> <a style='padding:10px;background-color:blue;color:#fff ;text-decoration:none' href ='https://localhost:7109/api/Authentication/ConfirmEmail?email={applicationUser.Email}&token={encodedToken}'> Verification Email </a>";
                        await sendMailService.SendMail(applicationUser.Email, "Spatium CMS Verification Email!", MailBody);

                        var response = new SpatiumResponse()
                        {
                            Message = ResponseMessages.UserCreatedSuccessfully,
                            Success = true
                        };
                        return Ok(response);
                    }
                    throw new SpatiumException(identityResult.Errors.Select(x => x.Description).ToArray());
                }
                return BadRequest(ModelState);
            });
        }
        [HttpPut]
        [Route("UpdateUser")]
        [Authorize]
        public Task<IActionResult> UpdateUser(UpdateUserRequest updateUserRequest)
        {
            return TryCatchLogAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    var userId = GetUserId();
                    var blogId = GetBlogId();
                    var user = await userManager.FindByIdAsync(userId) ?? throw new SpatiumException(ResponseMessages.UserNotFound);
                    if (updateUserRequest.ImageProfile != null)
                    {
                        if (updateUserRequest.ImageProfile.ContentType.ToLower() != "image/jpg" &&
                        updateUserRequest.ImageProfile.ContentType.ToLower() != "image/jpeg" &&
                        updateUserRequest.ImageProfile.ContentType.ToLower() != "image/x-png" &&
                        updateUserRequest.ImageProfile.ContentType.ToLower() != "image/png")
                            throw new SpatiumException("The Image You Are Selected Or Uploaded Is Not Valid!!");
                    }
                    string filePath = updateUserRequest.ImageProfile != null ? await attachmentService.SaveAttachment($"{blogId}/ImagesProfile/", updateUserRequest.ImageProfile, enviroenment.WebRootPath, userId) : user.ProfileImagePath;

                    var userUpdateInput = mapper.Map<ApplicationUserUpdateInput>(updateUserRequest);
                    userUpdateInput.ProfileImagePath = filePath;
                    user.Update(userUpdateInput);
                    await unitOfWork.SaveChangesAsync();
                    var response = new SpatiumResponse()
                    {
                        Message = ResponseMessages.UserUpdatedSuccessfully,
                        Success = true
                    };
                    return Ok(response);
                }
                return BadRequest(ModelState);
            });
        }


        [HttpPut]
        [Route("UpdateUserPassword")]
        [Authorize]
        public Task<IActionResult> UpdateUserPassword(UpdateUserPaswordRequest updateUserPasswordRequest)
        {
            return TryCatchLogAsync(async () =>
            {

                if (ModelState.IsValid)
                {
                    var userId = GetUserId();
                    var LoginUser = await userManager.FindByIdAsync(userId);

                    IdentityResult result = await userManager.ChangePasswordAsync(LoginUser, updateUserPasswordRequest.CurrenetPassword, updateUserPasswordRequest.NewPassword);

                    if (result.Succeeded)
                    {
                        await unitOfWork.SaveChangesAsync();
                        var response = new SpatiumResponse()
                        {
                            Message = ResponseMessages.PasswordChangedSuccessfully,
                            Success = true
                        };
                        return Ok(response);
                    }
                    throw new SpatiumException(result.Errors.Select(x => x.Description).ToArray());
                }
                return BadRequest(ModelState);
            });
        }


        [HttpGet]
        [Route("ChangeUserActivation/{userId}")]
        [Authorize(Roles = "Super Admin")]
        [PermissionFilter(PermissionsEnum.UpdateUser)]
        public Task<IActionResult> ChangeUserActivation(string userId, UserStatusEnum userStatus)
        {
            return TryCatchLogAsync(async () =>
            {
                var curentUserId = GetUserId();
                var blogId = GetBlogId();
                var user = await userManager.FindUserByIdInBlogIgnoreFilterAsync(blogId, userId) ?? throw new SpatiumException(ResponseMessages.UserNotFound);
                if (user.UserStatusId == (int)userStatus || user.UserStatusId == (int)UserStatusEnum.Pending || userId == curentUserId)
                    throw new SpatiumException("can not cahange user status !!");
                if ((int)userStatus == 3)
                    throw new SpatiumException("can not cahange user status to be pending !!");

                user.ChangeActivation(userStatus);
                await unitOfWork.SaveChangesAsync();
                var response = new SpatiumResponse()
                {
                    Message = ResponseMessages.UserStatusChanged,
                    Success = true,
                };
                return Ok(response);
            });
        }

        [HttpGet]
        [Route("GetUserDetails")]
        [Authorize]
        public Task<IActionResult> GetUserDetails()
        {
            return TryCatchLogAsync(async () =>
            {
                var userId = GetUserId();
                var blogId = GetBlogId();
                var currentuser = await userManager.FindUserInBlogByIdIncludingRole(blogId, userId) ?? throw new SpatiumException(ResponseMessages.UserNotFound);
                var detailesResult = mapper.Map<ViewUserProfileResult>(currentuser);
                return Ok(detailesResult);
            });
        }
        [HttpGet]
        [Route("GetAllUsers")]
        [Authorize(Roles = "Super Admin")]
        public Task<IActionResult> GetAllUsers([FromQuery] GetEntitiyParams entityParams)
        {
            return TryCatchLogAsync(async () =>
            {
                var blogId = GetBlogId();
                var userId = GetUserId();

                if ((entityParams.StartDate != null && entityParams.EndDate == null) || (entityParams.EndDate != null && entityParams.StartDate == null)) throw new SpatiumException("you sholud enter both of date start and end date");

                if (entityParams.StartDate > entityParams.EndDate || entityParams.EndDate < entityParams.StartDate) throw new SpatiumException("the datetime invalid !!");

                var users = await userManager.FindUsersInBlogIncludingRole(blogId, entityParams);
                if (users.IsNullOrEmpty())
                    throw new SpatiumException("there are not users !!");
                var detailesResult = mapper.Map<List<ViewUsersResponse>>(users);
                var usersCount = await userManager.GetUsersCount(blogId);
 
                return Ok(new
                {
                    Success = true,
                    Message = "Get All Users",
                    Data = detailesResult,
                    Total= usersCount.Count(),
                    PagesNumber= usersCount.Count() / entityParams.PageSize+1,
                }
                );
                
            });
        }
        [HttpGet]
        [Route("GetUsersAnalytics")]
        [Authorize(Roles = "Super Admin")]
        public Task<IActionResult> GetUsersAnalytics()
        {
            return TryCatchLogAsync(async () =>
            {
                var blogId = GetBlogId();
                var detailesResult = await userManager.GetBlogUersAnalytics(blogId) ?? throw new SpatiumException("there are not users !!");
                return Ok(detailesResult);
            });
        }
        [HttpGet]
        [Route("GetUserActivity")]
        [Authorize(Roles = "Super Admin")]
        [Authorize]
        public Task<IActionResult> GetUserActivity()
        {
            return TryCatchLogAsync(async () =>
            {
                var userId = GetUserId();
                var activity = await unitOfWork.RoleRepository.GetActivityLog(userId);
                var response = new List<ActivityResponse>();
                foreach (var item in activity)
                {
                    response.Add(new ActivityResponse()
                    {
                        Content = item.Content,
                        IconPath = configuration.GetSection("ApiBaseUrl").Value + "/" + item.LogIcon.Path,
                        Date = item.CreationDate
                    });
                }
                return Ok(response);
            });
        }

        [HttpGet]
        [Route("ResendInvitaion")]
        [Authorize(Roles = "Super Admin")]
        [PermissionFilter(PermissionsEnum.CreateUser)]
        public Task<IActionResult> ResendInvitaion(string UserId)
        {
            return TryCatchLogAsync(async () =>
            {

                var invatedUser = await userManager.FindByIdAsync(UserId);
                var suberAdminId = GetUserId();
                var blogId = GetBlogId();
                var user = await userManager.FindUserInBlogAsync(blogId, suberAdminId) ?? throw new SpatiumException("You Are Not In This Blog");
                if (invatedUser.ParentUserId != user.Id)
                    throw new SpatiumException("You Are Not Create This User ");
                if (invatedUser.EmailConfirmed == true)
                    throw new SpatiumException("This User Is Not Pending");

                var token = await userManager.GenerateEmailConfirmationTokenAsync(invatedUser);
                var encodedToken = WebUtility.UrlEncode(token);
                invatedUser.ChangeOTP(OTPGenerator.GenerateOTP());
                await unitOfWork.SaveChangesAsync();

                string MailBody = $"<h1> Your OTP is {invatedUser.OTP} </h1> <br/> <a style='padding:10px;background-color:blue;color:#fff ;text-decoration:none' href ='https://localhost:7109/api/Authentication/ConfirmEmail?email={invatedUser.Email}&token={encodedToken}'> Verification Email </a>";
                await sendMailService.SendMail(invatedUser.Email, "Spatium CMS Verification Email!", MailBody);

                var response = new SpatiumResponse()
                {
                    Message = ResponseMessages.UserCreatedSuccessfully,
                    Success = true
                };
                return Ok(response);


            });
        }

        [HttpPut]
        [Route("UpdateUserAsSuperAdmin")]
        [Authorize(Roles = "Super Admin")]
        public Task<IActionResult> UpdateUserAsSuperAdmin(UpdateUserAsSuperAdminRequest updateUserRequest)
        {
            return TryCatchLogAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    var userId = GetUserId();
                    var Admin = await userManager.FindByIdAsync(userId) ?? throw new SpatiumException("Invalid Super Admin Id");
                    var user = await userManager.FindByIdAsync(updateUserRequest.UserId) ?? throw new SpatiumException("Invalid User Id");

                    if (Admin.Id != user.ParentUserId)
                        throw new SpatiumException("You Can Not Update This User ");
                    if (updateUserRequest.RoleId == MainRolesIdsEnum.SuperAdmin.GetDescription())
                        throw new SpatiumException(" Blog Contain Onley Super Admin");

                    var userUpdateInput = mapper.Map<ApplicationUserUpdateInputSuperAdmin>(updateUserRequest);
                    user.Update(userUpdateInput);
                    await unitOfWork.SaveChangesAsync();
                    var response = new SpatiumResponse()
                    {
                        Message = ResponseMessages.UserUpdatedSuccessfully,
                        Success = true
                    };
                    return Ok(response);
                }
                return BadRequest(ModelState);
            });
        }

        // not complete 
        [HttpDelete]
        [Route("DeleteUserAsSuperAdmin")]
        [Authorize(Roles = "Super Admin")]
        public Task<IActionResult> DeleteUserAsSuperAdmin(string userId)
        {
            return TryCatchLogAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    var SuperAdminId = GetUserId();
                    var Admin = await userManager.FindByIdAsync(SuperAdminId) ?? throw new SpatiumException("Invalid Super Admin Id");
                    var user = await userManager.FindByIdAsync(userId) ?? throw new SpatiumException("Invalid User Id");

                    if (Admin.Id != user.ParentUserId)
                        throw new SpatiumException("You Can Not Delete This User ");

                    user.Delete();

                    await unitOfWork.SaveChangesAsync();
                    var response = new SpatiumResponse()
                    {
                        Message = "User Deleted Successfully",
                        Success = true
                    };
                    return Ok(response);
                }
                return BadRequest(ModelState);
            });
        }
    }
}
