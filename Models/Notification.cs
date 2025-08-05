// Models/Notification.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        public string Message { get; set; } = "";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }

        // Link only to Project
        [ForeignKey("Project")]
        public int Project_pkId { get; set; }
        public virtual Project Project { get; set; }
    }
}
