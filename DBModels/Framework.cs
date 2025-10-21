using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class Framework
{
    public int FrameworkPkId { get; set; }

    public string? FrameworkName { get; set; }

    public int LanguagePkId { get; set; }

    public virtual Language LanguagePk { get; set; } = null!;

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
