using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class VerifyOtpViewModel
    {
        [Required]
        public string RollNumber { get; set; }

        [Required(ErrorMessage = "OTP Code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP Code must be 6 digits")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP Code must be numeric")]
        [Display(Name = "OTP Code")]
        public string OTPCode { get; set; }
    }

}
