using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class Notification
    {
        [Key] // Explicitly define as primary key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
        public int Notification_pkId { get; set; }

        public int UserId { get; set; } // Student_pkId (foreign key)

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsRead { get; set; }

        [StringLength(50)]
        public string NotificationType { get; set; }

        [ForeignKey("UserId")]
        public Student Student { get; set; }
    }
}