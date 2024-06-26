﻿using AutoMapper;
using Domain.BlogsAggregate.Input;
using Domain.BlogsAggregate;
using Domian.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Spatium_CMS.Controllers.PostController.Response;
using Spatium_CMS.Controllers.PostController.Request;
using Microsoft.AspNetCore.Identity;
using Domain.ApplicationUserAggregate;
using Microsoft.AspNetCore.Authorization;
using Domain.Base;
using Utilities.Enums;
using Spatium_CMS.Filters;
using Infrastructure.Strategies.AuthorizationStrategy;
using Utilities.Results;
using Utilities.Exceptions;
using Infrastructure.Extensions;
using Infrastructure.Strategies.PostStatusStrategy.Factory;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.IdentityModel.Tokens;
namespace Spatium_CMS.Controllers.PostController
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : CmsControllerBase
    {
        private readonly IAuthorizationStrategyFactory authorizationStrategyFactory;
        private readonly IPostStatusFactory postStatusFactory;
        private readonly IConfiguration configuration;
        public PostController(ILogger<PostController> logger, IMapper mapper, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IAuthorizationStrategyFactory authorizationStrategyFactory, IPostStatusFactory postStatusFactory, IConfiguration configuration) : base(unitOfWork, mapper, logger, userManager)
        {
            this.authorizationStrategyFactory = authorizationStrategyFactory;
            this.postStatusFactory = postStatusFactory;
            this.configuration = configuration;
        }


        #region LikesAndShares
        //[HttpGet]
        //[Route("Like/{postId:int}")]
        //[Authorize]
        //[PermissionFilter(PermissionsEnum.ReadPost)]
        //public Task<IActionResult> Like(int postId)
        //{
        //    return TryCatchLogAsync(async () =>
        //    {
        //        var userId =  GetUserId();
        //        var blogId = GetBlogId();
        //        var user = await userManager.FindUserInBlogAsync(blogId, userId);
        //        if(user == null)
        //        {
        //            throw new SpatiumException("You Are Not In this Blog");
        //        }
        //        var post = await unitOfWork.BlogRepository.GetPostByIdAsync(postId, blogId);
        //        if (post == null)
        //        {
        //            throw new SpatiumException("Invalid Post Id");
        //        }
        //        post.IncrementLikeCount();
        //        await unitOfWork.SaveChangesAsync();
        //        var response = new SpatiumResponse()
        //        {
        //            Message = "Like Done Successfuly",
        //            Success = true
        //        };
        //        return Ok(response);
        //    });
        //}

        //[HttpGet]
        //[Route("DisLike/{postId:int}")]
        //[Authorize]
        //[PermissionFilter(PermissionsEnum.ReadPost)]
        //public Task<IActionResult> DisLike(int postId)
        //{
        //    return TryCatchLogAsync(async () =>
        //    {
        //        var userId = GetUserId();
        //        var blogId = GetBlogId();
        //        var user = await userManager.FindUserInBlogAsync(blogId, userId);
        //        if (user == null)
        //        {
        //            throw new SpatiumException("You Are Not In this Blog");
        //        }
        //        var post = await unitOfWork.BlogRepository.GetPostByIdAsync(postId, blogId);
        //        if (post == null)
        //        {
        //            throw new SpatiumException("Invalid Post Id");
        //        }
        //        post.DecrementLikeCount();
        //        await unitOfWork.SaveChangesAsync();
        //        var response = new SpatiumResponse()
        //        {
        //            Message = "DisLike Done Successfuly",
        //            Success = true
        //        };
        //        return Ok(response);
        //    });
        //}

        //[HttpGet]
        //[Route("Share/{postId:int}")]
        //[Authorize]
        //[PermissionFilter(PermissionsEnum.ReadPost)]
        //public Task<IActionResult> Share(int postId)
        //{
        //    return TryCatchLogAsync(async () =>
        //    {
        //        var userId = GetUserId();
        //        var blogId = GetBlogId();
        //        var user = await userManager.FindUserInBlogAsync(blogId, userId);
        //        if (user == null)
        //        {
        //            throw new SpatiumException("You Are Not In this Blog");
        //        }
        //        var post = await unitOfWork.BlogRepository.GetPostByIdAsync(postId, blogId);
        //        if (post == null)
        //        {
        //            throw new SpatiumException("Invalid Post Id");
        //        }
        //        post.IncrementShareCount();
        //        await unitOfWork.SaveChangesAsync();
        //        var response = new SpatiumResponse()
        //        {
        //            Message = "Share Done Successfuly",
        //            Success = true
        //        };
        //        return Ok(response);
        //    });
        //}
        //[HttpGet]
        //[Route("UnShare/{postId:int}")]
        //[Authorize]
        //[PermissionFilter(PermissionsEnum.ReadPost)]
        //public Task<IActionResult> UnShare(int postId)
        //{
        //    return TryCatchLogAsync(async () =>
        //    {
        //        var userId = GetUserId();
        //        var blogId = GetBlogId();
        //        var user = await userManager.FindUserInBlogAsync(blogId, userId);
        //        if (user == null)
        //        {
        //            throw new SpatiumException("You Are Not In this Blog");
        //        }
        //        var post = await unitOfWork.BlogRepository.GetPostByIdAsync(postId, blogId);
        //        if (post == null)
        //        {
        //            throw new SpatiumException("Invalid Post Id");
        //        }
        //        post.DecrementShareCount();
        //        await unitOfWork.SaveChangesAsync();
        //        var response = new SpatiumResponse()
        //        {
        //            Message = "UnShare Done Successfuly",
        //            Success = true
        //        };
        //        return Ok(response);
        //    });
        //}
        //[HttpGet]
        //[Route("GetTopLikePost")]
        //[Authorize]
        //[PermissionFilter(PermissionsEnum.ReadPost)]
        //public Task<IActionResult> GetPostById([FromQuery]int blogId ,[FromQuery] int count=5)
        //{
        //    return TryCatchLogAsync(async () =>
        //    {
        //        if (count <= 0)
        //            throw new SpatiumException("Count Must Be Greater Than 0");
        //        var userId = GetUserId();
        //        var user = await userManager.FindUserInBlogAsync(blogId , userId)?? throw new SpatiumException(ResponseMessages.PostNotFound);
        //        var AllPosts = await unitOfWork.BlogRepository.GetAllPostByBlogId(blogId);
        //        if (count > AllPosts.Count())
        //            throw new SpatiumException("Count Must less Than Total Posts Number");
        //        var posts = await unitOfWork.BlogRepository.GetTopLikePost(blogId, count);
        //        var result = mapper.Map<IEnumerable<PostRespone>>(posts);
        //        return Ok(result);
        //    });
        //}
        //[HttpGet]
        //[Route("GetTopSharePost")]
        //[Authorize]
        //[PermissionFilter(PermissionsEnum.ReadPost)]
        //public Task<IActionResult> GetTopSharePost([FromQuery] int blogId, [FromQuery] int count = 5)
        //{
        //    return TryCatchLogAsync(async () =>
        //    {
        //        if (count <= 0)
        //            throw new SpatiumException("Count Must Be Greater Than 0");
        //        var userId = GetUserId();
        //        var user = await userManager.FindUserInBlogAsync(blogId, userId) ?? throw new SpatiumException(ResponseMessages.PostNotFound);
        //        var AllPosts = await unitOfWork.BlogRepository.GetAllPostByBlogId(blogId);
        //        if (count > AllPosts.Count())
        //            throw new SpatiumException("Count Must less Than Total Posts Number");
        //        var posts = await unitOfWork.BlogRepository.GetTopSharePost(blogId, count);
        //        var result = mapper.Map<IEnumerable<PostRespone>>(posts);
        //        return Ok(result);
        //    });
        //} 
        #endregion 

        [HttpGet("{Id:int}")]
        //[Authorize]
        //[PermissionFilter(PermissionsEnum.ReadPost)]
        public Task<IActionResult> GetPostById(int Id)
        {
            return TryCatchLogAsync(async () =>
            {
                var post = await unitOfWork.BlogRepository.GetPostByIdAsync(Id) ?? throw new SpatiumException(ResponseMessages.PostNotFound);
                var result = mapper.Map<PostRespone>(post);
                result.ShareCount = post.Shares.Count;
                return Ok(result);
            });
        }

        [HttpGet]
        [Route("GetPostSnippetPreview/{id}")]
        [Authorize]
        [PermissionFilter(PermissionsEnum.ReadPost)]
        public Task<IActionResult> GetPostSnippetPreview(int id)
        {
            return TryCatchLogAsync(async () =>
            {
                var blogId = GetBlogId();
                var post = await unitOfWork.BlogRepository.GetPostByIdAsync(id,blogId) ?? throw new SpatiumException(ResponseMessages.PostNotFound);
                var result = mapper.Map<PostSnippetPreviewResponse>(post);
                return Ok(result);
            });
        }

        [HttpGet]
        [Route("GetPosts")]
        [Authorize]
        [PermissionFilter(PermissionsEnum.ReadPost)]
        public Task<IActionResult> GetPosts([FromQuery] GetEntitiyParams postParams)
        {
            return TryCatchLogAsync(async () =>
            {
                var userId = GetUserId();
                var blogId = GetBlogId();

                if ((postParams.StartDate != null && postParams.EndDate == null) || (postParams.EndDate != null && postParams.StartDate == null))   throw new SpatiumException("You should enter both of date start and end date");

                if (postParams.StartDate > postParams.EndDate || postParams.EndDate < postParams.StartDate)
                    throw new SpatiumException("The datetime invalid !!");

                var posts = await unitOfWork.BlogRepository.GetPostsAsync(postParams, blogId);
                if (posts.IsNullOrEmpty())
                    throw new SpatiumException("There are not posts !!");
                List<PostRespone> postsResponse = new List<PostRespone>();
                foreach (var item in posts)
                {
                    postsResponse.Add(mapper.Map<PostRespone>(item));
                }
                var totalPosts=await unitOfWork.BlogRepository.GetAllPostByBlogId(blogId);
              
                return Ok(new
                {
                    Success = true,
                    Message = "Get All Posts",
                    Data = postsResponse,
                    Total= totalPosts.Count(),
                    PagesNumber= totalPosts.Count()/ postParams.PageSize+1,
                });
                 
            });
        }


        [HttpPut]
        [Route("SchedulePost")]
        [Authorize]
        [PermissionFilter(PermissionsEnum.PublishPost)]
        public Task<IActionResult> SchedulePost(int postId, string ScheduledPublishDateTime, string ScheduledUnpublishDateTime)
        {
            return TryCatchLogAsync(async () =>
            {
                var blogId = GetBlogId();
                var post = await unitOfWork.BlogRepository.GetPostByIdAsync(postId, blogId) ?? throw new SpatiumException(ResponseMessages.PostNotFound);

                if (!DateTime.TryParse(ScheduledPublishDateTime, out DateTime scheduledPublishDateTime) ||
                    !DateTime.TryParse(ScheduledUnpublishDateTime, out DateTime scheduledUnpublishDateTime))
                {
                    throw new SpatiumException(ResponseMessages.InvalidDateFormat);
                }
                if (!(scheduledPublishDateTime < DateTime.UtcNow) && !(scheduledUnpublishDateTime <= DateTime.UtcNow) && !(scheduledUnpublishDateTime <= scheduledPublishDateTime) && !(scheduledPublishDateTime >= scheduledUnpublishDateTime))
                {
                    post.SchedualedPost(scheduledPublishDateTime, scheduledUnpublishDateTime);
                    await unitOfWork.SaveChangesAsync();
                    return Ok(mapper.Map<PostRespone>(post));
                }
                throw new SpatiumException("Invalid Schedule DateTime in PublishDateTime or UnpublishDateTime ");
            });
        }

        [HttpPut]
        [Route("PublishedPost")]
        [Authorize]
        [PermissionFilter(PermissionsEnum.PublishPost)]
        public Task<IActionResult> PublishedPost(int postId)
        {
            return TryCatchLogAsync(async () =>
            {
                var blogId = GetBlogId();
                var roleId = GetRoleId();
                var userId = GetUserId();
                var role = await unitOfWork.RoleRepository.GetRoleByIdAsync(roleId, blogId) ?? throw new SpatiumException(ResponseMessages.InvalidRole);
                var postStrategy = authorizationStrategyFactory.GetSelectStrategy(role, blogId, userId, postId);
                var queryExpression = postStrategy.GetPublishPostSelectExpression();
                var post = await unitOfWork.BlogRepository.GetPostByExpression(queryExpression) ?? throw new SpatiumException(ResponseMessages.PostNotFound);

                var postStatusStrategy = postStatusFactory.GetStrategy(roleId);
                var postStatus = postStatusStrategy.GetPostStatus();
                if (post.StatusId == (int)postStatus)
                    throw new SpatiumException($"Post is already {postStatus}");
                post.ChangePostStatus(postStatus);
                await unitOfWork.SaveChangesAsync();
                return Ok(mapper.Map<PostRespone>(post));
            });
        }

        [HttpPut]
        [Route("UnPublishedPost")]
        [Authorize]
        [PermissionFilter(PermissionsEnum.UnpublishPost)]
        public Task<IActionResult> UnPublishedPost(int postId)
        {
            return TryCatchLogAsync(async () =>
            {
                var blogId = GetBlogId();
                var post = await unitOfWork.BlogRepository.GetPostByIdAsync(postId, blogId) ?? throw new SpatiumException(ResponseMessages.PostNotFound);
                post.ChangePostStatus(PostStatusEnum.Unpublished);
                await unitOfWork.SaveChangesAsync();
                return Ok(mapper.Map<PostRespone>(post));
            });
        }

        [HttpPost]
        [Authorize]
        [PermissionFilter(PermissionsEnum.CreatePost)]
        public Task<IActionResult> Create([FromQuery]CreatePostRequest createPostRequest)
        {
            return TryCatchLogAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    var userId = GetUserId();
                    var blogId = GetBlogId();

                    var blog = await unitOfWork.BlogRepository.GetByIdAsync(blogId);
                    var subscription = await unitOfWork.SubscriptionRepository.GetSubscriptionByIdAsync(blog.SubscriptionId);

                    if (subscription.NumberOfPosts <= blog.Posts.Count()) throw new SpatiumException("Sorry the Blog Reached The Limitation Of Posts To Create");

                    var postInput = mapper.Map<PostInput>(createPostRequest);
                    foreach (var item in createPostRequest.TableOfContentRequests)
                    {
                        var tableOfContentInput = mapper.Map<TableOfContentInput>(item);
                        postInput.TableOfContents.Add(tableOfContentInput);
                    }
                    postInput.CreatedById = userId;
                    postInput.BlogId = blogId;
                    postInput.CommentsAllowed = true;
                    var post = new Post(postInput);

                    await unitOfWork.BlogRepository.CreatePostAsync(post);
                    await unitOfWork.SaveChangesAsync();

                    return Ok(new SpatiumResponse()
                    {
                        Message = $"Post {createPostRequest.Title} Created Successfully!",
                        Success = true
                    });
                }
                return BadRequest(ModelState);
            });

        }
        [HttpPut]
        [Authorize]
        [PermissionFilter(PermissionsEnum.UpdatePost)]
        public Task<IActionResult> Update(UpdatePostRequest updatePostRequest)
        {
            return TryCatchLogAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    var userId = GetUserId();
                    var blogId = GetBlogId();
                    var roleId = GetRoleId();

                    var role = await unitOfWork.RoleRepository.GetRoleByIdAsync(roleId, blogId) ?? throw new SpatiumException(ResponseMessages.InvalidRole);
                    var author = await userManager.FindUserInBlogAsync(blogId, userId) ?? throw new SpatiumException(ResponseMessages.AuthorNotFound);
                    var user = await userManager.FindUserInBlogAsync(blogId, userId) ?? throw new SpatiumException(ResponseMessages.UserNotFound);

                    var strategy = authorizationStrategyFactory.GetEditStrategy(role, blogId, userId, updatePostRequest.Id);
                    var expression = strategy.GetUpdatePostExpression();

                    var post = await unitOfWork.BlogRepository.GetPostByExpression(expression) ?? throw new SpatiumException(ResponseMessages.PostNotFound);
                    var postInput = mapper.Map<UpdatePostInput>(updatePostRequest);
                    foreach (var tableOfContent in updatePostRequest.TableOfContentRequests)
                    {
                        var contentUpdate = mapper.Map<UpdateTableOfContentInput>(tableOfContent);
                        postInput.UpdateTableOfContentInput.Add(contentUpdate);
                    }
                    post.Update(postInput);

                    await unitOfWork.SaveChangesAsync();

                    var response = new SpatiumResponse()
                    {
                        Message = $"Post {post.Title} Updated Successfully!",
                        Success = true,
                    };
                    return Ok(response);
                }
                return BadRequest(ModelState);
            });
        }

        [HttpDelete]
        [Authorize]
        [PermissionFilter(PermissionsEnum.DeletePost)]
        public Task<IActionResult> Remove(int id)
        {
            return TryCatchLogAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    var blogId = GetBlogId();
                    var roleId = GetRoleId();
                    var userId = GetUserId();

                    var role = await unitOfWork.RoleRepository.GetRoleByIdAsync(roleId, blogId) ?? throw new SpatiumException(ResponseMessages.InvalidRole);

                    var strategy = authorizationStrategyFactory.GetDeleteStartegy(role, blogId, userId, id);
                    var expression = strategy.GetDeletePostExpression();
                    var post = await unitOfWork.BlogRepository.GetPostByExpression(expression) ?? throw new SpatiumException(ResponseMessages.PostNotFound);
                    post.Delete();
                    await unitOfWork.SaveChangesAsync();
                    return Ok(new SpatiumResponse()
                    {
                        Message = $"Post {post.Title} Deleted Successfully!",
                        Success = true
                    });
                }
                return BadRequest(ModelState);
            });
        }

        [HttpPut]
        [Route("ChangeCommentsAllowed")]
        [Authorize]
        [PermissionFilter(PermissionsEnum.CreatePost)]
        public Task<IActionResult> ChangeCommentsAllowed(int PostId, bool IsAllowed)
        {
            return TryCatchLogAsync(async () =>
            {
                if (ModelState.IsValid)
                {
                    var blogId = GetBlogId();
                    var post = await unitOfWork.BlogRepository.GetPostByIdAsync(PostId, blogId) ?? throw new SpatiumException(ResponseMessages.PostNotFound);
                    post.ChangeAllowedComments(IsAllowed);
                    await unitOfWork.SaveChangesAsync();
                    return Ok(new SpatiumResponse()
                    {
                        Message = $"Post {post.Title} Comments Allowed Is Changed Successfully!",
                        Success = true
                    });
                }
                return BadRequest(ModelState);
            });
        }

        #region Like
        [HttpGet]
        [Route("like/{postId}")]
        //[Authorize]
        //[PermissionFilter(PermissionsEnum.ReadPost)]
        public Task<IActionResult> Like(int postId)
        {
            return TryCatchLogAsync(async () =>
            {
                var userId = GetUserId();
                var blogId = GetBlogId();
                var user = await userManager.FindUserInBlogAsync(blogId, userId) ?? throw new SpatiumException("Your Are not Allow To Make Like in This Blog");
                var post = await unitOfWork.BlogRepository.GetPostByIdAsync(postId, blogId) ?? throw new SpatiumException(ResponseMessages.PostNotFound);

                var likeModel = await unitOfWork.BlogRepository.GetLiketByPostIdAndCreatedByIdAsync(postId, userId);

                if (likeModel is not null)
                {
                    await unitOfWork.BlogRepository.DeleteLiketAsync(likeModel);
                    return Ok(new SpatiumResponse
                    {
                        Message = " DisLike Created Successfully",
                        Success = true,

                    });
                }

                var LikeInput = new LikeInput()
                {
                    CreatedbyId = userId,
                    PostId = postId,
                };
                var like = new Like(LikeInput);

                await unitOfWork.BlogRepository.CreateLiketAsync(like);
                await unitOfWork.SaveChangesAsync();

                return Ok(new SpatiumResponse
                {
                    Message = " Like Created Successfully",
                    Success = true,

                });

            });
        }
        #endregion
        #region External Shars 
        [HttpGet]
        [Authorize(Roles = "Super Admin")]
        [Route("Share/{postId}")]

        public Task<IActionResult> Share(int postId)
        {
            return TryCatchLogAsync(async () =>
            {
                var post = await unitOfWork.BlogRepository.GetPostByIdAsync(postId) ?? throw new SpatiumException(ResponseMessages.PostNotFound);
                if (post.StatusId != (int)PostStatusEnum.Published) throw new SpatiumException("Post Must Be Published");

                var share = new Share(postId);
                await unitOfWork.BlogRepository.CreateSharetAsync(share);
                await unitOfWork.SaveChangesAsync();

                var Url = configuration.GetSection("ApiBaseUrl").Value + $"/api/Post/ExternalShared/{postId}";

                return Ok(new SpatiumResponse()
                {
                    Message = $"{Url}",
                    Success = true,
                });
            });
        }
        // integration  preview
        [HttpGet()]
        [Route("ExternalShared/{postId}")]
        //[Authorize]
        //[PermissionFilter(PermissionsEnum.ReadPost)]
        public Task<IActionResult> ExternalShared(int postId)
        {
            return TryCatchLogAsync(async () =>
            {
                var post = await unitOfWork.BlogRepository.GetPostByIdAsync(postId) ?? throw new SpatiumException(ResponseMessages.PostNotFound);
                if (post.StatusId != (int)PostStatusEnum.Published) throw new SpatiumException("Post Must Be Published");


                var result = mapper.Map<ExternalPostResponse>(post);
                return Ok(result);
            });
        }
        #endregion

    }
}