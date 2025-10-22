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

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public DateTime DonationDate { get; set; } = DateTime.UtcNow;

        // Optional: store Stripe session id/reference for auditing
        [StringLength(200)]
        public string? StripeSessionId { get; set; }
    }
}
