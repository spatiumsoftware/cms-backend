﻿using System.ComponentModel.DataAnnotations;

namespace Spatium_CMS.Controllers.PostController.Request
{
    public class CreatePostRequest
    {
        [Required]
        [MinLength(3, ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Slug is required")]
        [MinLength(3, ErrorMessage = "Slug minimum length is 3")]
        public string Slug { get; set; }

        [Required(ErrorMessage = "image path is required")]
        public string FeaturedImagePath { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [MinLength(100, ErrorMessage = "Content minimum length is 100")]
        public string Content { get; set; }

        [Required]
        [MaxLength(250, ErrorMessage = "Max Length is 250 letter")]
        public string MetaDescription { get; set; }

        public int ContentLineSpacing { get; set; } = 20;

        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }

        public string Tag { get; set; }

        [Required(ErrorMessage = "Author Id is Required")]
        public string AuthorId { get; set; }

        public bool CommentsAllowed { get; set; } = true;

        public List<TableOfContentRequest> TableOfContentRequests { get; set; } = new List<TableOfContentRequest>();
    }
}
