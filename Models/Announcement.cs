// In Models/Announcement.cs
using System.ComponentModel.DataAnnotations;

public class Announcement
{
    [Key]
    public int AnnouncementId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; }  // Added title field

    [Required]
    [StringLength(1000)]  // Increased length
    public string Message { get; set; }

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; } = DateTime.Now;

    [Display(Name = "Expiry Date")]
    public DateTime? ExpiryDate { get; set; }

    [Display(Name = "Block Submissions?")]
    public bool BlocksSubmissions { get; set; }

    // Calculated property to check if announcement is active
    public bool IsActive => DateTime.Now >= StartDate &&
                          (ExpiryDate == null || DateTime.Now <= ExpiryDate);
}