using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class AuditLog
{
    public int LogPkId { get; set; }

    public string? StudentName { get; set; }

    public int StudentPkId { get; set; }

    public string? Action { get; set; }

    public string? PerformedBy { get; set; }

    public DateTime? PerformedAt { get; set; }

    public virtual Student StudentPk { get; set; } = null!;
}
