using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class OTP
    {
        [Key]
        public int OTP_PkId { get; set; }

        [Required]
        public string RollNumber { get; set; }

        [Required]
        public string OTPCode { get; set; }

        public DateTime SendTime { get; set; }

    }

}
