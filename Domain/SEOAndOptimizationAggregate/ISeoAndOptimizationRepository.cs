
namespace Domain.SEOAndOptimizationAggregate
{
    public interface ISeoAndOptimizationRepository
    {
        Task<bool> AllowedSEOAndOptimization(int blogId);
    }
}
