using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class Nrctype
{
    public int NrctypePkId { get; set; }

    public string TypeCode { get; set; } = null!;

    public string TypeDescription { get; set; } = null!;

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
