using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class ProjectType
{
    public int ProjectTypePkId { get; set; }

    public string TypeName { get; set; } = null!;

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
