using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class Notification
{
    public int NotificationPkId { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public string NotificationType { get; set; } = null!;

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedDate { get; set; }

    public int? ProjectPkId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Project? ProjectPk { get; set; }

    public virtual Student User { get; set; } = null!;
}
