using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class Email
{
    public int EmailPkId { get; set; }

    public string EmailAddress { get; set; } = null!;

    public string RollNumber { get; set; } = null!;

    public string Class { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public int? AcademicYearPkId { get; set; }

    public DateTimeOffset CreatedDate { get; set; }

    public virtual AcademicYear? AcademicYearPk { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
