using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class ProjectMember
{
    public int ProjectMemberPkId { get; set; }

    public string? Role { get; set; }

    public bool? IsDeleted { get; set; }

    public int StudentPkId { get; set; }

    public int ProjectPkId { get; set; }

    public string? RoleDescription { get; set; }

    public virtual Project ProjectPk { get; set; } = null!;

    public virtual Student StudentPk { get; set; } = null!;
}
