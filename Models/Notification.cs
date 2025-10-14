using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Notification_pkId { get; set; }  // Primary Key

        // Foreign Key to Student (User receiving the notification)
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual Student Student { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        public bool IsRead { get; set; } = false;

        [StringLength(50)]
        public string NotificationType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedDate { get; set; }

        // Optional link to a Project
        public int? Project_pkId { get; set; }

        [ForeignKey(nameof(Project_pkId))]
        public virtual Project? Project { get; set; }

      
    }
}
