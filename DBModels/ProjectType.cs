using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class ProjectType
{
    public int ProjectTypePkId { get; set; }

    public string? TypeName { get; set; }

    public virtual ICollection<Language> Languages { get; set; } = new List<Language>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
