using Infrastructure.Services.AuthinticationService.Models;

namespace Spatium_CMS.Controllers.AuthenticationController.Response
{
    public class LoginResponse
    {
        public LoggedInUser Data { get; set; }
        public string RoleName { get; set; }
        public string ImageProfilePath { get; set; }

    }
}
