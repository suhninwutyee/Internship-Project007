namespace ProjectManagementSystem.Models
{
    public class NotificationViewModel
    {
        public int Notification_pkId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsRead { get; set; }
        public string NotificationType { get; set; } // e.g., "Project", "Task", "System"
        public int? RelatedEntityId { get; set; } // ID of related project/task/etc
    }
}