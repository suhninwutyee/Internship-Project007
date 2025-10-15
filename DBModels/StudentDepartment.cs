using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class StudentDepartment
{
    public int DepartmentPkId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
