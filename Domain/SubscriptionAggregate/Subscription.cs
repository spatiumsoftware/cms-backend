using Domain.Base;
using Domain.BlogsAggregate;

namespace Domain.SubscriptionAggregate
{
    public class Subscription :EntityBase
    {
        #region Prop
        public string  Title { get;private set; }
        public string SubTitle { get; private set; }
        public decimal Price { get; private set; }
        public int Duration {  get; private set; }
        public bool IsDefault  { get; private  set; }
        public int NumberOfUsers { get;private  set; }
        public bool SEO_Usage { get; private set; }
        public int? NumberOfPosts { get;private set; }
        public int? StorageCapacity { get;private set; }

        #endregion

        #region Nav list

        private readonly List<Blog> _blogs = new List<Blog>();
        public virtual IReadOnlyList<Blog> blogs { get => _blogs.ToList(); }

        #endregion

    }
}
