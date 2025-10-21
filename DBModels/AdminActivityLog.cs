using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class AdminActivityLog
{
    public int Id { get; set; }

    public string AdminId { get; set; } = null!;

    public string? LoggedName { get; set; }

    public string? Action { get; set; }

    public string? Details { get; set; }

    public DateTime? Timestamp { get; set; }

    public string? IpAddress { get; set; }

    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();
}
