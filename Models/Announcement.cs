// In Models/Announcement.cs
using System.ComponentModel.DataAnnotations;

public class Announcement
{
    [Key]
    public int Id { get; set; } = 1; // Single announcement with ID=1

    [Required, StringLength(200)]
    public string Title { get; set; } = "Submission Status";

    [Required, StringLength(1000)]
    public string Message { get; set; } = "Current submission guidelines";

    [Display(Name = "Start Date")]
    public DateTime StartDate { get; set; } = DateTime.Now;

    [Display(Name = "End Date")]
    public DateTime? EndDate { get; set; }

    [Display(Name = "Block Submissions?")]
    public bool BlockSubmissions { get; set; }

    // Calculated property to check if announcement is active
    public bool IsActive => DateTime.Now >= StartDate &&
                         (EndDate == null || DateTime.Now <= EndDate.Value);
}