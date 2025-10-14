using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class AdminActivityLog
    {
        [Key]
        public int Id { get; set; }

        public string AdminId { get; set; }  // Stores the user ID (optional)

        [Required]
        [Display(Name = "Name Used")]
        public string LoggedName { get; set; }  // Stores the exact name entered

        [Required]
        public string Action { get; set; }

        public string Details { get; set; }  // Optional field

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string IpAddress { get; set; }

        public ICollection<Announcement>? Announcements { get; set; }

    }
}