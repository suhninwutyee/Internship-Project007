using System.ComponentModel.DataAnnotations;

public class Announcement
{
    [Key]
    public int AnnouncementId { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [Required]
    [StringLength(1000)]
    public string Message { get; set; }

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; } = DateTime.Now;

    [Display(Name = "Expiry Date")]
    public DateTime? ExpiryDate { get; set; }

    [Display(Name = "Block Submissions?")]
    public bool BlocksSubmissions { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } // Manual activation control

    // Helper property for display purposes
    public bool IsCurrentlyEffective => IsActive &&
                                      DateTime.Now >= StartDate &&
                                      (ExpiryDate == null || DateTime.Now <= ExpiryDate);
}