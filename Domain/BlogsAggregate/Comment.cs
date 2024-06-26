﻿using Domain.ApplicationUserAggregate;
using Domain.Base;
using Domain.BlogsAggregate.Input;
using Domain.LookupsAggregate;
using Utilities.Enums;

namespace Domain.BlogsAggregate
{
    public class Comment:EntityBase
    {
        public string Content { get; private set; }
        public int? ParentCommentId { get; private set; }
        public int PostId { get; private set; }
        public int StatusId { get; private set; }
        public string CreatedbyId { get; private set; }

        #region Navigational Properties
        public virtual Comment ParentComment { get; private set; }
        public virtual Post Post { get; private set; }
        public virtual ApplicationUser Createdby { get; private set; }
        public virtual CommentStatus Status { get; private set; }
        #endregion

        #region Virtual Lists
        private readonly List<Comment> _comments = new List<Comment>();
        public virtual IReadOnlyList<Comment> Comments => _comments;
        #endregion

        #region CTOR
        public Comment()
        {
            
        }
        public Comment(CommentInput commentInput)
        {
            this.CreationDate = DateTime.UtcNow;
            this.IsDeleted = false;
            this.ParentCommentId = commentInput.ParentCommentId == null ? null : commentInput.ParentCommentId.Value;
            this.Content = commentInput.Content;
            this.PostId = commentInput.PostId;
            this.CreatedbyId = commentInput.CreatedById;
            this.StatusId = (int)CommentStatusEnum.Pending;
        }
        #endregion

        public void ChangeCommentStatus(CommentStatusEnum status)
        {
            this.StatusId = (int) status;
        }

        public  void Update(CommentUpdateInput commentInput)
        {
            this.Content = commentInput.Content;
            this.StatusId = commentInput.StatusId;
        }
        public void Delete()
        {
            IsDeleted = true;
        }

        public void ChangeStatus(CommentStatusEnum status) {
            StatusId = (int)status;
        }
     
    }
}
