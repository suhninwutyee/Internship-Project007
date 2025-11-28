using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class Otp
{
    public int OtpPkId { get; set; }

    public string RollNumber { get; set; } = null!;

    public string Otpcode { get; set; } = null!;

    public DateTime SendTime { get; set; }

    public bool IsUsed { get; set; }

    public DateTime ExpiryTime { get; set; }
}
