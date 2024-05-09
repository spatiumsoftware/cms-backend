using Domain.ApplicationUserAggregate;
using Domain.BlogsAggregate;
using Domain.SEOAndOptimizationAggregate;
using Domain.StorageAggregate;
using Domain.SubscriptionAggregate;

namespace Domian.Interfaces
{
    public interface IUnitOfWork: IDisposable
    {
        #region Repos
        public IBlogRepository BlogRepository { get; }
        //public ITableOfContent TableOfContentRepository { get; }
        //public IPostRepository PostRepository { get; }
        //public ICommentRepository CommentRepository { get; }
        public IUserRoleRepository RoleRepository { get; }
        IStorageRepository StorageRepository { get; }
        ISubscriptionRepository SubscriptionRepository { get; }
        ISeoAndOptimizationRepository SeoAndOptimizationRepository { get; }
        #endregion
        Task SaveChangesAsync();
         IQueryable<Folder> GetFolderFamaily(int folderId,int blogId);

    }
}
