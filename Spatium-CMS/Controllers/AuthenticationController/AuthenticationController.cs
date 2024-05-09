using AutoMapper;
using Domain.ApplicationUserAggregate;
using Domain.BlogsAggregate;
using Domain.Interfaces;
using Domain.StorageAggregate;
using Domain.StorageAggregate.Input;
using Domian.Interfaces;
using Infrastructure.Extensions;
using Infrastructure.Services.AuthinticationService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Spatium_CMS.Controllers.AuthenticationController.Converter;
using Spatium_CMS.Controllers.AuthenticationController.Request;
using Spatium_CMS.Controllers.AuthenticationController.Response;
using Spatium_CMS.Filters;
using Utilities.Enums;
using Utilities.Exceptions;
using Utilities.Extensions;
using Utilities.Results;

namespace Spatium_CMS.Controllers.AuthenticationController
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : CmsControllerBase
    {
        private readonly IAuthenticationService authenticationService;
      
        private readonly RoleManager<UserRole> roleManager;

        public AuthenticationController(ILogger<AuthenticationController> logger, IAuthenticationService authenticationService, IMapper mapper, RoleManager<UserRole> roleManager, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager) : base(unitOfWork, mapper, logger, userManager)
        {
            this.authenticationService = authenticationService;
            this.roleManager = roleManager;
        }

        [HttpPost]
        [Route("Register")]
        public Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            return TryCatchLogAsync(async () =>
            {
                var converter = new AuthenticationConverter(mapper);
                var role = await roleManager.Roles.FirstOrDefaultAsync(x => x.Id == MainRolesIdsEnum.SuperAdmin.GetDescription());
                var userInput = converter.GetApplicationUserInput(request, role.Id);
                userInput.JobTitle = request.FullName;
                userInput.ProfileImagePath = null;
                // make quer get def SubId Then Path It 
                var sub =await unitOfWork.SubscriptionRepository.GetDefaultSubscriptionAsync();
                userInput.SubscriptionId= sub.Id;
                var newUser = new ApplicationUser(userInput);
                var result = await authenticationService.Register(newUser, request.Password);
                if (result.Success)
                {
                    var stroageInput = new StorageInput()
                    {
                        ApplicationUserId = newUser.Id,
                        BlogId = newUser.BlogId,
                        Capacity = "100000000000000"
                    };
                    var storage=new Storage(stroageInput);
                    await unitOfWork.StorageRepository.AddStorage(storage);
                    await unitOfWork.SaveChangesAsync();
                    return Ok(new RegisterResponse()
                    {
                        Message = result.Message,
                        Email = newUser.Email,
                        Token = result.Data
                    });
                }
                return BadRequest(new FailedRegisterResponse()
                {
                    Message = result.Message,
                    Email = newUser.Email,
                });
            });
        }

        [HttpPost]
        [Route("Login")]
        public Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            return TryCatchLogAsync(async () =>
            {
                var result = await authenticationService.Login(request.Email, request.Password);
                if(result.PaymentState== false)
                {
                    return BadRequest(result);
                }

                if (result.Success)
                {
                    var user=await userManager.FindByEmailAsync(request.Email);
                    var loginuserdetailes = new UserLogInDetailes();
                    loginuserdetailes.ImageProfilePath = user.ProfileImagePath;
                    loginuserdetailes.RoleName = user.Role.Name;
                    var UserLogInDetailes = mapper.Map<UserLogInDetailes>(user);

                    return Ok(new LoginResponse
                    {   
                        Data = result.Data,
                        ImageProfilePath = UserLogInDetailes.ImageProfilePath,
                        RoleName = UserLogInDetailes.RoleName
                    });
                }
                if (result.Data != null && !result.Data.EmailConfirmed)
                {
                    return BadRequest(new ResendOtpResponse()
                    {
                        Token = result.Data.Token,
                        Message = result.Message,
                        Email = request.Email,
                    });
                }
                return Unauthorized(result.Message);
            });
        }

        [HttpPost]
        [Route("ConfirmEmail")]
        public Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
        {
            return TryCatchLogAsync(async () =>
            {
                if (request.Token.IsNullOrEmpty() || request.OTP.IsNullOrEmpty() || request.Email.IsNullOrEmpty())
                    throw new SpatiumException("Invalid Data");
                var result = await authenticationService.ConfirmEmail(request.Email, request.Token, request.OTP);
                if (result.Success)
                {
                    var response = new ConfirmEmailResponse()
                    {
                        Message = result.Message,
                        Email = request.Email,
                    };
                    return Ok(response);
                }
                return BadRequest(result.Message);
            });
        }

        [HttpPost]
        [Route("ResendOTP")]
        public Task<IActionResult> ResendOTP(string email)
        {
            return TryCatchLogAsync(async () =>
            {
                if (email.IsNullOrEmpty())
                    throw new SpatiumException("Email is required");
                var result = await authenticationService.ResendConfirmationEmailOTP(email);
                if (result.Success)
                {
                    var response = new ResendOtpResponse()
                    {
                        Token = result.Data,
                        Message = result.Message,
                        Email = email
                    };
                    return Ok(response);
                }
                throw new SpatiumException(result.Message);
            });
        }
        [HttpPost]
        [Route("ResendOTPForgetPassword")]
        public Task<IActionResult> ResendOTPForgetPassword(string email)
        {
            return TryCatchLogAsync(async () =>
            {
                if (email.IsNullOrEmpty())
                    throw new SpatiumException("Email is required");
                var result = await authenticationService.ResendOTPForgetPassword(email);
                if (result.Success)
                {
                    var response = new ResendOtpResponse()
                    {
                        Token = result.Data,
                        Message = result.Message,
                        Email = email
                    };
                    return Ok(response);
                }
                throw new SpatiumException(result.Message);
            });
        }

        [HttpPost]
        [Route("ForgetPassword")]
        public Task<IActionResult> ForgetPassword(string email)
        {
            return TryCatchLogAsync(async () =>
            {
                if (email.IsNullOrEmpty())
                    throw new SpatiumException("Email is required");
                var result = await authenticationService.ForgetPassword(email);
                if (result.Success)
                {
                    var converter = new AuthenticationConverter(mapper);
                    var response = converter.GetForgetPasswordResponse(result, email);
                    return Ok(response);
                }
                var failedResponse = new FailedForgetPasswordResponse()
                {
                    Message = result.Message,
                    Email = email
                };
                return Ok(failedResponse);
            });
        }

        [HttpPost]
        [Route("ConfirmForgetPassword")]
        public Task<IActionResult> ConfirmForgetPassword(ConfirmForgetPassword request)
        {
            return TryCatchLogAsync(async () =>
            {
                if (!request.NewPassword.Equals(request.ConfirmNewPassword))
                    return BadRequest(new ConfirmForgetPasswordResponse()
                    {
                        Email = request.Email,
                        Message = ResponseMessages.PasswordDoesnotMatch
                    });
                var result = await authenticationService.ConfirmForgetPassword(request.Email, request.Token, request.NewPassword);
                if (result.Success)
                {
                    return Ok(new ConfirmForgetPasswordResponse()
                    {
                        Email = request.Email,
                        Message = result.Message
                    });
                }
                return Unauthorized(new ConfirmForgetPasswordResponse()
                {
                    Email = request.Email,
                    Message = result.Message
                });
            });
        }

        [HttpPost]
        [Route("ConfirmForgetPasswordOTP")]
        public Task<IActionResult> ConfirmForgetPasswordOTP(ConfirmForgetPasswordOTP confirmOtp)
        {
            return TryCatchLogAsync(async () =>
            {
                var result = await authenticationService.ConfirmForgetPasswordOTP(confirmOtp.Email, confirmOtp.OTP);
                if (result.Success)
                    return Ok(new ConfirmForgetPasswordOtpResponse()
                    {
                        Email = confirmOtp.Email,
                        Message = result.Message,
                        Token = result.Data
                    });
                return BadRequest(result.Message);
            });
        }

        //[HttpDelete("{userId}")]
        //public async Task<IActionResult> DeleteUser(string userId)
        //{
        //    try
        //    {
        //        var BlogId = GetBlogId();
        //        await authenticationService.DeleteUserAndRelatedData(userId, BlogId);
        //        await unitOfWork.SaveChangesAsync();
        //        return Ok("User and related data deleted successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred while deleting user: {ex.Message}");
        //    }
        //}


        [HttpDelete]
        [Route("Delete")]
        [Authorize]
        [PermissionFilter(PermissionsEnum.DeleteMedia)]
        public Task<IActionResult> DeleteUser()
        {
            return TryCatchLogAsync(async () =>
            {
                if (ModelState.IsValid)
                { 
                    var BlogId = GetBlogId();
                    var userId = GetUserId();
                    var user = await userManager.FindByIdAsync(userId);
                    await authenticationService.DeleteUserStorage(userId, BlogId);
                    await unitOfWork.RoleRepository.TryDeleteCustomRoleAsync(BlogId,userId);
                    var posts = await unitOfWork.BlogRepository.GetAllPostByBlogId(BlogId);
                    foreach (var post in posts)
                    {
                        await unitOfWork.BlogRepository.TryDeletePostsAsync(post);
                    }
                    // user
                    await unitOfWork.RoleRepository.FinalDeleteUser(BlogId,userId);

                    // blog 
                    var blog = await unitOfWork.BlogRepository.GetByIdAsync(BlogId);
                    await unitOfWork.BlogRepository.TryDeleteBlogsAsync(blog);
                    await unitOfWork.SaveChangesAsync();
                    return Ok("User and related data deleted successfully.");
                }
                   return BadRequest(ModelState);
            });
        }

    }
}
