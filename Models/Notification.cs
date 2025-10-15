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

        // Foreign Key to Student (User receiving the notification)
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual Student Student { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        //public DateTime CreatedDate { get; set; }

        [StringLength(50)]
        public string NotificationType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Nullable DateTime?


        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedDate { get; set; }

        //[ForeignKey("UserId")]
        //public Student Student { get; set; }

        // Link only to Project
        [ForeignKey("Project")]
        public int? Project_pkId { get; set; }
        public virtual Project Project { get; set; }

        //public int? AdminActivityLogId { get; set; }

        //// Foreign key to AdminActivityLog
        //[ForeignKey(nameof(AdminActivityLogId))]
        
        
        //public virtual AdminActivityLog? Admin { get; set; }
        public bool IsRead { get; set; }
    }
}
