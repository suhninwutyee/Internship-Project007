using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class AuditLog
{
    public int LogPkId { get; set; }

    public string StudentName { get; set; } = null!;

    public int StudentPkId { get; set; }

    public string Action { get; set; } = null!;

    public string PerformedBy { get; set; } = null!;

    public DateTime PerformedAt { get; set; }

    public virtual Student StudentPk { get; set; } = null!;
}
