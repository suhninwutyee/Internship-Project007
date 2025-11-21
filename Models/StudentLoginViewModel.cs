using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.DBModels
{
    public class StudentLoginViewModel
    {
        [Required]
        public string RollNumber { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; } 

        public bool RememberMe { get; set; } // optional if you want to implement

    }
}
