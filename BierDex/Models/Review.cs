using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using BierDex.Services;

namespace BierDex.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(1000, MinimumLength = 5)]
        public string Content { get; set; }
        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars.")]
        public int Rating { get; set; }

        [Required]
        public string Username { get; set; }

        // Navigation property
        [Required]
        public Beer Beer { get; set; }

        [Required]
        public IdentityUser User { get; set; }

        // Foreign Key
        [Required]
        public int BeerId { get; set; }
        [Required]
        public string UserId { get; set; }

        public Review(string content, int rating, IdentityUser user, Beer beer)
        {
            this.Content = content;
            this.Rating = rating;
            this.Username = user.UserName;
            this.User = user;
            this.UserId = user.Id;
            this.Beer = beer;
            this.BeerId = beer.Id;
        }

        public Review() { }
    }
}
