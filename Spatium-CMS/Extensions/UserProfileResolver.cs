using AutoMapper;
using Domain.ApplicationUserAggregate;
using Infrastructure.Services.AuthinticationService.Models;
using Spatium_CMS.Controllers.AuthenticationController.Response;

namespace Spatium_CMS.Extensions
{
    public class LoginUserResolver : IValueResolver<ApplicationUser, UserLogInDetailes, string>
    {
        private readonly IConfiguration configration;
        public LoginUserResolver(IConfiguration configration)
        {
            this.configration = configration;
        }
        public string Resolve(ApplicationUser source, UserLogInDetailes destination, string destMember, ResolutionContext context)
        {
            if (!string.IsNullOrEmpty(source.ProfileImagePath))
            {
                return $"{configration["ApiBaseUrl"]}/{source.ProfileImagePath}";
            }
            else
                return string.Empty;
        }
    }
}
