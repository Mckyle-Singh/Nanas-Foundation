using System.ComponentModel.DataAnnotations;

namespace Nanas_Foundation.Models
{
    public class BlogPost
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Title { get; set; }

        [Required]
        public string AuthorName { get; set; }

        [EmailAddress]
        public string AuthorEmail { get; set; }

        [Url]
        public string WebsiteLink { get; set; }

        [Required]
        public string PdfFilePath { get; set; } // Path to uploaded PDF

        public string ProfilePhotoPath { get; set; } // Optional author image

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
