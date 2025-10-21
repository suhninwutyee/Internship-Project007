using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class Student
{
    public int StudentPkId { get; set; }

    public string? PhoneNumber { get; set; }

    public int NrcPkId { get; set; }

    public int NrctypePkId { get; set; }

    public int? Nrcnumber { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public int DepartmentPkId { get; set; }

    public int AcademicYearPkId { get; set; }

    public int EmailPkId { get; set; }

    public string? StudentName { get; set; }

    public string? ProfilePhotoUrl { get; set; }

    public virtual AcademicYear AcademicYearPk { get; set; } = null!;

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual StudentDepartment DepartmentPk { get; set; } = null!;

    public virtual Email EmailPk { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual Nrctownship NrcPk { get; set; } = null!;

    public virtual Nrctype NrctypePk { get; set; } = null!;

    public virtual ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();

    public virtual ICollection<Project> ProjectStudentPks { get; set; } = new List<Project>();

    public virtual ICollection<Project> ProjectSubmittedByStudentPks { get; set; } = new List<Project>();
}
