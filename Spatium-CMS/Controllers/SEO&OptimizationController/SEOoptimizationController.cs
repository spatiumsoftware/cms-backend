using AutoMapper;
using Domain.ApplicationUserAggregate;
using Domian.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Spatium_CMS.Controllers.ReportsAndAnalytics;
using Spatium_CMS.Filters;
using Utilities.Enums;
using Utilities.Exceptions;
using Utilities.Results;

namespace Spatium_CMS.Controllers.SEO_OptimizationController
{
    [Route("api/[controller]")]
    [ApiController]
    public class SEOoptimizationController : CmsControllerBase
    {
        public SEOoptimizationController(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ReportsAndAnalyticsController> logger) : base(unitOfWork, mapper, logger, userManager)
        {
        }


        [HttpGet]
        [Route("AllowedSEOAndOptimization")]
        [Authorize(Roles = "Super Admin")]
        [PermissionFilter(PermissionsEnum.ReadSEO)]
        public Task<IActionResult> AllowedSEOAndOptimization()
        {
            return TryCatchLogAsync(async () =>
            {
                var blogId = GetBlogId();
                var seoAllowed = await unitOfWork.SeoAndOptimizationRepository.AllowedSEOAndOptimization(blogId);
                if (!seoAllowed) throw new SpatiumException("your subscription plan not allowed SEO And Optimization");
                return Ok(new SpatiumResponse()
                {
                    Message = "You Are Allowed To SEO Optimization",
                    Success = true
                });
            });
        }
    }
}
