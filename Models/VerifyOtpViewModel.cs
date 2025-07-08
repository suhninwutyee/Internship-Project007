using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class VerifyOtpViewModel
    {
        [Required]
        public string RollNumber { get; set; }

        [Required]
        public string OTPCode { get; set; }
    }

}
