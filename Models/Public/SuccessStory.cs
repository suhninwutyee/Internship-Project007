using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace ProjectManagementSystem.Models
{
    public class SuccessStory
    {
        public int Id { get; set; }

        [Required]
        public string StudentName { get; set; }

        [Required]
        public string InternshipCompany { get; set; }

        [Required]
        public string Story { get; set; }

        // Store uploaded image filename
        public string ImageFileName { get; set; }

        // Upload image file (not saved to DB)
        [NotMapped]
        [Display(Name = "Upload Image")]
        public IFormFile ImageFile { get; set; }

        // Only for form validation
        [NotMapped]
        [Required]
        [Display(Name = "Access Code")]
        public string AccessCode { get; set; }
    }
}
