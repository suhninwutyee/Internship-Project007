namespace ProjectManagementSystem.DBModels
{
    public class NotificationViewModel
    {
        public int Id { get; set; }
        public string Message { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; } = "";
        public bool? IsRead { get; set; }
        //public bool? IsRead { get; set; }
        public string Title { get; set; }
        public string NotificationType { get; set; }
        public string DeadlineStatus { get; set; } = "";
    }
}
