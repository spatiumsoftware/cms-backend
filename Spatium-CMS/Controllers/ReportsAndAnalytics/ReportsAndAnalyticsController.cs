using AutoMapper;
using Domain.ApplicationUserAggregate;
using Domian.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Spatium_CMS.Controllers.ReportsAndAnalytics
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsAndAnalyticsController : CmsControllerBase
    {
        public ReportsAndAnalyticsController(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ReportsAndAnalyticsController> logger) : base(unitOfWork, mapper, logger, userManager)
        {
        }


    }
}
