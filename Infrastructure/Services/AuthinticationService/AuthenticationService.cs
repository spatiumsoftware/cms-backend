﻿using Castle.Core.Configuration;
using Domain.ApplicationUserAggregate;
using Domain.Interfaces;
using Domian.Interfaces;
using Infrastructure.Extensions;
using Infrastructure.Services.AuthinticationService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Utilities.Enums;
using Utilities.Exceptions;
using Utilities.Extensions;
using Utilities.Helpers;
using Utilities.Results;

namespace Infrastructure.Services.AuthinticationService
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<UserRole> roleManager;
        private readonly ISendMailService mailService;
        private readonly AuthConfig authConfig;
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IUnitOfWork unitOfWork;

        public AuthenticationService(UserManager<ApplicationUser> userManager, RoleManager<UserRole> roleManager, ISendMailService mailService, AuthConfig jwtSetting, ILogger<AuthenticationService> logger, IUnitOfWork unitOfWork)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.mailService = mailService;
            this.authConfig = jwtSetting;
            _logger = logger;
            this.unitOfWork = unitOfWork;

        }

        public async Task<ApplicationUser> GetUserDetailes(string userId)
        {
            return await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<SpatiumResponse> ConfirmForgetPassword(string email, string token, string newPassword)
        {
            _logger.LogInformation("Confirm Change Email started for email {email} at {time}", email, DateTime.UtcNow);
            var user = await userManager.FindByEmailAsync(email);
            var decodedToken = WebUtility.UrlDecode(token);
            if (user != null)
            {
                var result = await userManager.ResetPasswordAsync(user, decodedToken, newPassword);
                if (result.Succeeded)
                {
                    user.ClearOTP();
                    await userManager.UpdateAsync(user);
                    return new SpatiumResponse()
                    {
                        Success = true,
                        Message = ResponseMessages.PasswordChangedSuccessfully
                    };
                }
                return new SpatiumResponse()
                {
                    Success = false,
                    Message = string.Join('\n', result.Errors.Select(x => x.Description).ToArray())
                };
            }
            return new SpatiumResponse()
            {
                Success = false,
                Message = ResponseMessages.InvalidEmail
            };
        }

        private async Task<SpatiumResponse> ConfirmOtp(ApplicationUser user, string otp)
        {
            if (user.OTP != null && user.OTP.Equals(otp))
            {
                //(3 Minute For developing purpose) it should 30 minutes
                if (DateTime.UtcNow < user.OTPGeneratedAt.Value.AddMinutes(3))
                {
                    user.ClearOTP() ;
                    await userManager.UpdateAsync(user);
                    return new SpatiumResponse()
                    {
                        Success = true,
                        Message = ResponseMessages.OtpConfirmed
                    };
                }
                user.ChangeOTP(OTPGenerator.GenerateOTP());
                await userManager.UpdateAsync(user);
                await mailService.SendMail(user.Email, "Spatium CMS Verification Email!", $"Your OTP is: {user.OTP}.");
                return new SpatiumResponse()
                {
                    Success = false,
                    Message = ResponseMessages.OTPExpired
                };
            }
            return new SpatiumResponse()
            {
                Success = false,
                Message = ResponseMessages.InvalidOTP
            };
        }

        public async Task<SpatiumResponse> ConfirmEmail(string email, string token, string otp)
        {
            _logger.LogInformation("Confirm Email started for email {email} at {time}", email, DateTime.UtcNow);
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                if (user.EmailConfirmed)
                {
                    _logger.LogInformation("Email is already confirmed {email} at {time}", email, DateTime.UtcNow);

                    return new SpatiumResponse
                    {
                        Success = false,
                        Message = ResponseMessages.EmailIsAlreadyConfirmed
                    };
                }

                var otpConfirmationResult = await ConfirmOtp(user, otp);
                if (otpConfirmationResult.Success)
                {
                    var decodedToken = WebUtility.UrlDecode(token);
                    var identityResult = await userManager.ConfirmEmailAsync(user, decodedToken);

                    if (identityResult.Succeeded)
                    {
                        user.ClearOTP();
                        _logger.LogInformation("Email Confirmed forr user {user} at {time}", user, DateTime.UtcNow);
                        user.ChangeActivation(UserStatusEnum.Active);
                        await userManager.UpdateAsync(user);
                        // welecome message 
                        if(user.RoleId == MainRolesIdsEnum.SuperAdmin.GetDescription())
                            await mailService.SendMail(user.Email, "Spatium CMS Welcome!", $"Welcome to the Spatium CMS ");

                        return new SpatiumResponse
                        {
                            Success = true,
                            Message = ResponseMessages.EmailConfirmedSuccessfully
                        };
                    }

                    return new SpatiumResponse
                    {
                        Success = false,
                        Message = string.Join('\n', identityResult.Errors.Select(x => x.Description).ToArray()),
                    };
                }
            }
            _logger.LogInformation("Email Not Found or Invalid OTP {email} at {time}", email, DateTime.UtcNow);
            return new SpatiumResponse()
            {
                Success = false,
                Message = ResponseMessages.InvalidOTP
            };
        }

        public async Task<SpatiumResponse<string>> ForgetPassword(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                user.ChangeOTP(OTPGenerator.GenerateOTP());
                await mailService.SendMail(user.Email, "Spatium CMS Verification Email!", $"Your OTP is: {user.OTP}.");
                await userManager.UpdateAsync(user);
                return new SpatiumResponse<string>()
                {
                    Success = true,
                    Message = ResponseMessages.ForgetPasswordEmailSent
                };
            }
            return new SpatiumResponse<string>()
            {
                Success = false,
                Message = ResponseMessages.ForgetPasswordEmailSent
            };
        }

        public async Task<SpatiumResponse<LoggedInUser>> Login(string email, string password)
        {
            _logger.LogInformation("Login started for user {email} at {time}", email, DateTime.UtcNow);
            var user = await userManager.FindByEmailAsync(email);
            if (user != null && await userManager.CheckPasswordAsync(user, password))
            {
                if (!user.EmailConfirmed)
                {
                    var otpResult = await ResendConfirmationEmailOTP(user.Email);
                    return new SpatiumResponse<LoggedInUser>()
                    {
                        Success = false,
                        Message = ResponseMessages.EmailNotConfirmed,
                        Data = new LoggedInUser()
                        {
                            Token = otpResult.Data
                        },
                    };
                }

                if(user.UserStatusId == (int)UserStatusEnum.DeActive)
                {
                    return new SpatiumResponse<LoggedInUser>()
                    {
                        Success = false,
                        Message = "You Are Not Active Yet!",
 
                    };
                }
                //check unpaid 
                var defaultSubscription = await unitOfWork.SubscriptionRepository.GetDefaultSubscriptionAsync();
                if (user.Blog.SubscriptionId != defaultSubscription.Id)
                {
                    var billHistory = await unitOfWork.SubscriptionRepository.GetAllBillingHistoryAsync(user.BlogId);
                    var currentmoth = DateTime.UtcNow.Month;
                    var lastbayment = billHistory.LastOrDefault();
                    if (lastbayment != null)
                    {
                        var month = lastbayment.CreationDate.Month;
                       if (month != currentmoth)
                        {
                            if(user.RoleId == MainRolesIdsEnum.SuperAdmin.GetDescription())
                            {
                                // send email 
                                var tok = await GenerateToken(user);
                                await mailService.SendMail(user.Email, "Spatium CMS ", $"your account is disabled, please renew your subscription to be able to use the system again ");

                                return new SpatiumResponse<LoggedInUser>
                                {
                                    Success = false,
                                    Message = "your account is disabled, please renew your subscription to be able to use the system again",
                                    PaymentState = false ,
                                    Data = new LoggedInUser
                                    {
                                        Token = tok.Token,
                                        ExpireDate = tok.ExpireDate,
                                        Email = user.Email,
                                        FullName=user.FullName ,
                                        
                                    }
                                };
                            }
                            else
                            {
                                return new SpatiumResponse<LoggedInUser>
                                {
                                    Success = false,
                                    Message = "Your Account is disabled, Please Contact your Admin.",
                                    PaymentState = false
                                };
                            }
                            
                        }
                    }
                }


                var tokenParams = await GenerateToken(user);
                var loggedInUser = new LoggedInUser()
                {
                    Email = email,
                    ExpireDate = tokenParams.ExpireDate,
                    FullName = user.FullName,
                    Token = tokenParams.Token,
                    EmailConfirmed = true,
                  

                };
                return new SpatiumResponse<LoggedInUser>()
                {
                    Success = true,
                    Data = loggedInUser,
                    
                };
            }
            return new SpatiumResponse<LoggedInUser>()
            {
                Success = false,
                Message = ResponseMessages.InvalidEmailOrPassword,
            };
        }

        public async Task<SpatiumResponse<string>> Register(ApplicationUser newUser, string password)
        {
            _logger.LogInformation("Registration Started. user: {user}", newUser);

            //var user = await userManager.FindByEmailAsync(newUser.Email);
            newUser.ChangeOTP(OTPGenerator.GenerateOTP());

            var createUserResult = await userManager.CreateAsync(newUser, password);

            if (createUserResult.Succeeded)
            {
                var token = await userManager.GenerateEmailConfirmationTokenAsync(newUser);
                var encodedToken = WebUtility.UrlEncode(token);
                _logger.LogInformation("User Created Successfully. user {user} at {date}", newUser, DateTime.UtcNow);
                await mailService.SendMail(newUser.Email, "Spatium CMS Verification Email!", $"Your OTP is: {newUser.OTP}.");
                return new SpatiumResponse<string>()
                {
                    Success = true,
                    Message = ResponseMessages.UserCreatedSuccessfully,
                    Data = encodedToken
                };
            }

            _logger.LogInformation("Registration failed with {errors} user: {newUser.Email}", createUserResult.Errors, newUser.Email);
            return new SpatiumResponse<string>()
            {
                Success = false,
                Message = string.Join(System.Environment.NewLine, createUserResult.Errors.Select(x => x.Description).ToArray())
            };
        }

        public async Task<SpatiumResponse<string>> ResendConfirmationEmailOTP(string email)
        {
            _logger.LogInformation("Resend Confirmation Email OTP Started for email: {email}", email);
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                if (user.EmailConfirmed)
                {
                    return new SpatiumResponse<string>()
                    {
                        Message = ResponseMessages.EmailIsAlreadyConfirmed,
                        Success = false,
                    };
                }
                if (user.OTPGeneratedAt != null && DateTime.UtcNow < user.OTPGeneratedAt.Value.AddSeconds(30))
                {
                    return new SpatiumResponse<string>()
                    {
                        Message = ResponseMessages.OtpWaitingPeroidError,
                        Success = false
                    };
                }
                var newOtp = OTPGenerator.GenerateOTP();
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebUtility.UrlEncode(token);
                user.ChangeOTP(newOtp);
                await userManager.UpdateAsync(user);
                await mailService.SendMail(user.Email, "Spatium CMS Verification Email!", $"Your OTP is: {user.OTP}.");
                _logger.LogInformation("ResendConfirmationEmailOTP: Confirmation Email OTP Started for user: {user}", user);
                return new SpatiumResponse<string>()
                {
                    Message = ResponseMessages.VerificationEmailSent,
                    Success = true,
                    Data = encodedToken
                };
            }
            return new SpatiumResponse<string>()
            {
                Message = ResponseMessages.VerificationEmailSent,
                Success = true,
            };
        }

        private async Task<TokenParameters> GenerateToken(ApplicationUser user)
        {
            _logger.LogDebug("Generating Token for user {email} at {time}", user.Email, DateTime.UtcNow);
            var permissions = await unitOfWork.RoleRepository.GetRolePermissionIds(user.RoleId);
            var claims = new List<Claim>()
            {
                new (ClaimTypes.NameIdentifier,user.Id),
                new (ClaimTypes.Email,user.Email),
                new ("RoleId",user.RoleId),
                new (ClaimTypes.Role,user.Role.Name),
                new ("BlogId",user.BlogId.ToString())
            };
            foreach (var item in permissions)
            {
                claims.Add(new Claim("Permissions", item.ToString()));
            }
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authConfig.SecretKey));
            var token = new JwtSecurityToken(
                    issuer: authConfig.ValidIssuer,
                    audience: authConfig.ValidAudience,
                    expires: DateTime.UtcNow.AddDays(authConfig.TokenExpireInDays),
                    claims: claims,
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
                );
            var tokenStr = new JwtSecurityTokenHandler().WriteToken(token);
            var tokenExpireDate = token.ValidTo;
            _logger.LogInformation("Token Generated for user: {email} at {time}", user.Email, DateTime.UtcNow);
            return new TokenParameters(tokenStr, tokenExpireDate);
        }

        public async Task<SpatiumResponse<string>> ConfirmForgetPasswordOTP(string email, string otp)
        {
            _logger.LogInformation("confirm Forget Password OTP Started for email: {email}", email);

            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var otpConfirmationResult = await ConfirmOtp(user, otp);
                if (otpConfirmationResult.Success)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var encodedToken = WebUtility.UrlEncode(token);
                    user.ClearOTP();
                    await userManager.UpdateAsync(user);
                    _logger.LogInformation("OTP Confirmed email: {email}", email);

                    return new SpatiumResponse<string>
                    {
                        Success = true,
                        Data = encodedToken,
                        Message = ResponseMessages.OtpConfirmed
                    };
                }

                return new SpatiumResponse<string>()
                {
                    Message = ResponseMessages.InvalidOTP,
                    Success = false,
                };
            }

            return new SpatiumResponse<string>()
            {
                Message = ResponseMessages.InvalidEmail,
                Success = false,
            };
        }


        public async Task DeleteUserStorage(string userId, int blogId)
        {

            var user = await userManager.FindUserInBlogAsync(blogId, userId);
            if (user != null)
            {
                var storage=await unitOfWork.StorageRepository.GetStorageByBlogId(blogId);
                var Folder = await unitOfWork.StorageRepository.GetAllFoldersbystorageandBlogAsync(storage.Id, blogId);

                foreach (var folder in Folder)
                {
                    await unitOfWork.StorageRepository.DeleteFolderAsync(folder.Id, blogId);
                    var files = await unitOfWork.StorageRepository.GetStaticFilesbyFolderIdStorageIdAsync(folder.Id, blogId);
                    foreach (var file in files)
                    {
                        await unitOfWork.StorageRepository.DeleteFileAsync(file.Id, blogId);
                    }
                }
                  await unitOfWork.StorageRepository.DeleteStorageForUserAsync(user.Id);
            }
            else
            {
                throw new SpatiumException($"User with ID {userId} not found.");
            }
        }

        #region Resned otp forget password

        public async Task<SpatiumResponse<string>> ResendOTPForgetPassword(string email)
        {
            _logger.LogInformation("Resend  OTP Forget Password Started for email: {email}", email);
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
            {

                if (user.OTPGeneratedAt != null && DateTime.UtcNow < user.OTPGeneratedAt.Value.AddSeconds(30))
                {
                    return new SpatiumResponse<string>()
                    {
                        Message = ResponseMessages.OtpWaitingPeroidError,
                        Success = false
                    };
                }
                var newOtp = OTPGenerator.GenerateOTP();
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var encodedToken = WebUtility.UrlEncode(token);
                user.ChangeOTP(newOtp);
                await userManager.UpdateAsync(user);
                await mailService.SendMail(user.Email, "Spatium CMS Verification Email!", $"Your OTP is: {user.OTP}.");
                _logger.LogInformation("ResendOTPForgetPassword: Confirmation Email OTP Started for user: {user}", user);
                return new SpatiumResponse<string>()
                {
                    Message = ResponseMessages.VerificationEmailSent,
                    Success = true,
                    Data = encodedToken
                };
            }
            return new SpatiumResponse<string>()
            {
                Message = ResponseMessages.VerificationEmailSent,
                Success = true,
            };
        }
        #endregion
    }
}
