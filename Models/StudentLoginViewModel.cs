using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class StudentLoginViewModel
    {
        [Required]
        public string RollNumber { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
    }
}
