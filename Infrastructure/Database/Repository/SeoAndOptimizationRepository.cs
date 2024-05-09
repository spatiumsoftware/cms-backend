
using Domain.SEOAndOptimizationAggregate;
using Infrastructure.Database.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database.Repository
{
    public class SeoAndOptimizationRepository : RepositoryBase, ISeoAndOptimizationRepository
    {
        public SeoAndOptimizationRepository(SpatiumDbContent SpatiumDbContent) : base(SpatiumDbContent)
        {
        }
        public async Task<bool> AllowedSEOAndOptimization(int blogId)
        {
            var blog=await SpatiumDbContent.Blogs.FirstOrDefaultAsync(b=>b.Id==blogId);
            var allowedSeo=blog.Subscription.SEO_Usage;
            return allowedSeo;
        }
    }
}
