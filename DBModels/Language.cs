using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class Language
{
    public int LanguagePkId { get; set; }

    public string? LanguageName { get; set; }

    public int ProjectTypePkId { get; set; }

    public int ProjectTypePkId1 { get; set; }

    public virtual ICollection<Framework> Frameworks { get; set; } = new List<Framework>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
