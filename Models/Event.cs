using System.ComponentModel.DataAnnotations;

namespace Nanas_Foundation.Models
{
    public class Event
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public string Location { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
