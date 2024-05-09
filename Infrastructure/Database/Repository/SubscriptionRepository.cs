using Domain.Base;
using Domain.LookupsAggregate;
using Domain.SubscriptionAggregate;
using Infrastructure.Database.Database;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Utilities.Exceptions;
namespace Infrastructure.Database.Repository
{
    public class SubscriptionRepository : RepositoryBase, ISubscriptionRepository
    {
        public SubscriptionRepository(SpatiumDbContent SpatiumDbContent):base(SpatiumDbContent)
        {
           
        }

        public async Task<IEnumerable<Subscription>> GetAllSubscriptionAsync()
        {
            return await SpatiumDbContent.Subscription.ToListAsync();
        }
        public async Task<Subscription> GetDefaultSubscriptionAsync()
        {
            return await SpatiumDbContent.Subscription.FirstOrDefaultAsync(s => s.IsDefault == true);
        }
        public async Task<Subscription> GetSubscriptionByIdAsync(int subscriptionId)
        {
            return await SpatiumDbContent.Subscription.FindAsync(subscriptionId);
        }
        public async Task<bool> CheckAllowSubscription(int blogId, int subscriptionId)
        {
            var blog = await SpatiumDbContent.Blogs.FirstOrDefaultAsync(b=>b.Id==blogId);
            var currentSubscription =await  SpatiumDbContent.Subscription.FirstOrDefaultAsync(s => s.Id == blog.SubscriptionId);
            var currentSubscriptionCapacity = GetBlogFilesCapcity(blogId);

            var newSubscription = await SpatiumDbContent.Subscription.FirstOrDefaultAsync(s=>s.Id==subscriptionId) ?? throw new SpatiumException("There Are No Subscription With This Id..");
            var date = blog.SubscriptionAt.AddMonths(1).AddDays(2);
            if (blog.SubscriptionId == subscriptionId && blog.SubscriptionAt.Date.AddMonths(1).AddDays(2) >= DateTime.UtcNow) throw new SpatiumException("You Are Already subscribed in this Plan");

            //check the old featuers with the new featuers 
            if (blog.Users.Count - 1 > newSubscription.NumberOfUsers) throw new SpatiumException($"To Perform This Action You Must delete {(blog.Users.Count - 1) - newSubscription.NumberOfUsers} Users");

            if (blog.Posts.Count > newSubscription.NumberOfPosts) throw new SpatiumException($"To Perform This Action You Must delete {blog.Posts.Count - newSubscription.NumberOfPosts} Posts");

            if (currentSubscriptionCapacity > newSubscription.StorageCapacity) throw new SpatiumException($"To Perform This Action You Must Free {currentSubscriptionCapacity - newSubscription.StorageCapacity} MB");

            return true;
        }
        private double GetBlogFilesCapcity(int blogId)
        {
            var files = SpatiumDbContent.Files.Where(f => f.BlogId == blogId);
            if (files == null)
            {
                return 0.0;
            }
            var capcity = 0.0;
            foreach (var f in files)
            {
                capcity += double.Parse(f.FileSize);
            }
            return capcity / 1024 / 1024;
        }
        public async Task<Subscription> GetSubscriptionByBlogIdAsync(int blogId)
        {
            var blog = await SpatiumDbContent.Blogs.FirstOrDefaultAsync(b => b.Id == blogId);
            return await  SpatiumDbContent.Subscription.FirstOrDefaultAsync(s => s.Id == blog.SubscriptionId);
        }
        #region BillHisory
        public async Task<IEnumerable<BillingHistory>> GetAllBillingHistory(GetEntityWithRange entitiyParams, int blogId)
        {
            var query = SpatiumDbContent.BillingHistory.Include(h => h.Subscription).Include(h => h.CreatedBy).Include(h => h.PaymentType).Where(b => b.BlogId == blogId).AsQueryable();

            if (!string.IsNullOrEmpty(entitiyParams.FilterColumn))
            {
                if (entitiyParams.FilterColumn.ToLower() == "paymenttypeid")
                {
                    query = query.Where(p => p.PaymentTypeId.ToString() == entitiyParams.FilterValue);
                }

                if (!string.IsNullOrEmpty(entitiyParams.FilterValue) && entitiyParams.StartDate == null && entitiyParams.EndDate == null && entitiyParams.FilterColumn.ToLower() != "paymenttypeid")
                {
                    query = query.ApplyFilter(entitiyParams.FilterColumn, entitiyParams.FilterValue);
                }
                if (entitiyParams.StartDate != null && entitiyParams.EndDate != null && entitiyParams.FilterColumn.ToLower() == "expirationdate")
                {
                    query = query.Where(p => p.CreationDate.AddMonths(1) >= entitiyParams.StartDate && p.CreationDate.AddMonths(1) <= entitiyParams.EndDate);
                }
                if (entitiyParams.StartValue > 0 && entitiyParams.EndValue > 0 && entitiyParams.FilterColumn.ToLower() == "ammount")
                {
                    query = query.Where(p => p.Ammount >= entitiyParams.StartValue && p.Ammount <= entitiyParams.EndValue);
                }

            }

            if (!string.IsNullOrEmpty(entitiyParams.SearchColumn) && !string.IsNullOrEmpty(entitiyParams.SearchValue))
            {
                query = query.ApplySearch(entitiyParams.SearchColumn, entitiyParams.SearchValue);
            }

            var paginatedQuery = query.Skip((entitiyParams.Page - 1) * entitiyParams.PageSize).Take(entitiyParams.PageSize);
            return await paginatedQuery.ToListAsync();

        }
        public async Task<IEnumerable<BillingHistory>> GetAllBillingHistoryAsync(int BlogId)
        {
            return await SpatiumDbContent.BillingHistory.Where(h => h.BlogId == BlogId).Include(h => h.Subscription).Include(h => h.CreatedBy).Include(h => h.PaymentType).ToListAsync();
        }

        public async Task InsertIntoHistory(BillingHistory history)
        {
           await SpatiumDbContent.BillingHistory.AddAsync(history);
        }

        public async Task<PaymentType> GetPaymentTypeAsync(int id)
        {
            return await SpatiumDbContent.PaymentTypes.SingleOrDefaultAsync(p => p.Id == id);
        }
        public async Task<BillingHistory> GetAllBillingHistoryByIdAsync(int Id)
        {
            return await SpatiumDbContent.BillingHistory.Include(b => b.PaymentType).FirstOrDefaultAsync(h => h.Id == Id);
        }
        #endregion



        //public async Task SwitchSubscription(int blogId, int newSubscriptionId)
        //{
        //    var blog=await SpatiumDbContent.Blogs.FindAsync(blogId);
        //    var oldSubscription=blog.Subscription;
        //    var newSubscription=await SpatiumDbContent.Subscription.FindAsync(newSubscriptionId);
        //    if (oldSubscription != null && newSubscription != null)
        //    {
        //        if (blog.Users.Count>=newSubscription.NumberOfUsers)
        //        {
        //            throw new SpatiumException("you can not ");
        //        }
        //    }

        //}
    }
}
