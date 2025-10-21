using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class AcademicYear
{
    public int AcademicYearPkId { get; set; }

    public string? YearRange { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
