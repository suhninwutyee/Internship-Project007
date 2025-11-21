//using System.ComponentModel.DataAnnotations;
//using Microsoft.AspNetCore.Http;
//using System.ComponentModel.DataAnnotations.Schema;
//using ProjectManagementSystem.Models;

//public class Announcement
//{
//    [Key]
//    public int AnnouncementId { get; set; }

//    [Required]
//    [StringLength(200)]
//    public string Title { get; set; }

//    [Required]
//    [StringLength(1000)]
//    public string Message { get; set; }

//    [Display(Name = "Created Date")]
//    public DateTime CreatedDate { get; set; } = DateTime.Now;

//    [Display(Name = "Start Date")]
//    public DateTime StartDate { get; set; } = DateTime.Now;

//    [Display(Name = "Expiry Date")]
//    public DateTime? ExpiryDate { get; set; }

//    [Display(Name = "Block Submissions?")]
//    public bool BlocksSubmissions { get; set; }

//    [StringLength(255)]
//    public string? FilePath { get; set; } // File URL or path

//    [NotMapped]
//    [Display(Name = "Attachment")]
//    public IFormFile? Attachment { get; set; }

//    //AdminActivityLog.Id
//    public int? AdminActivityLogId { get; set; }

//    [ForeignKey(nameof(AdminActivityLogId))]
//    public AdminActivityLog? Admin { get; set; }


//    public bool IsActive { get; set; }
//    //public bool IsActive => DateTime.Now >= StartDate &&
//    //                      (ExpiryDate == null || DateTime.Now <= ExpiryDate);
//}
