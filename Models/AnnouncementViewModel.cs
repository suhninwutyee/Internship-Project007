using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class AnnouncementViewModel
    {
        public int AnnouncementId { get; set; }
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;

        public DateTime? CreatedDate { get; set; }  
        public DateTime? StartDate { get; set; }       
        public DateTime? ExpiryDate { get; set; }        
        public bool? BlocksSubmissions { get; set; }   

        public string? FilePath { get; set; }
        public int? AdminActivityLogId { get; set; }

        // Computed property
        public bool IsActive => (StartDate ?? DateTime.Now) <= DateTime.Now &&
                                (ExpiryDate == null || ExpiryDate >= DateTime.Now);
    }
}
