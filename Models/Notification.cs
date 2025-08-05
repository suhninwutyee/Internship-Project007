// Models/Notification.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class Notification
    {
        [Key] // Explicitly define as primary key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
        public int Notification_pkId { get; set; }
        [Key]
        public int NotificationId { get; set; }

        public int UserId { get; set; } // Student_pkId (foreign key)

        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        public bool IsRead { get; set; } = false;

        [Required]
        public string Message { get; set; }

        public DateTime CreatedDate { get; set; }

        [StringLength(50)]
        public string NotificationType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }

        [ForeignKey("UserId")]
        public Student Student { get; set; }
        // Link only to Project
        [ForeignKey("Project")]
        public int Project_pkId { get; set; }
        public virtual Project Project { get; set; }
    }
}