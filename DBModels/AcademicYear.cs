using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class AcademicYear
{
    public int AcademicYearPkId { get; set; }

    public string YearRange { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Email> Emails { get; set; } = new List<Email>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
