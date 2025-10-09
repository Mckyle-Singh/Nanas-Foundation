using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Nanas_Foundation.Models
{
    public class Volunteer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        public Guid EventId { get; set; }
        public Event Event { get; set; }

        public DateTime SignedUpAt { get; set; } = DateTime.Now;

    }
}
