// Models/LoginModel.cs
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please enter your name")]
        [Display(Name = "Your Name")]
        public string Name { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}