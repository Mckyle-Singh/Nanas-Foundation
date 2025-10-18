using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nanas_Foundation.Models
{
    public class Donation
    {
        [Key]
        public int DonationID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(1, 10000000, ErrorMessage = "Please enter a valid donation amount.")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(100)]
        public string Bank { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 12, ErrorMessage = "Card number must be between 12 and 50 characters.")]
        public string CardNumber { get; set; } = string.Empty; // will be hashed before saving in production

        [Required]
        [StringLength(100)]
        public string NameOnCard { get; set; } = string.Empty;

        [Required]
        [StringLength(10, MinimumLength = 3, ErrorMessage = "CVC must be 3-4 digits.")]
        public string CVC { get; set; } = string.Empty; // will be hashed before saving

        [Required]
        public DateTime DonationDate { get; set; } = DateTime.UtcNow;
    }
}
