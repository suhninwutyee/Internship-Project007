using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class Nrctype
{
    public int NrctypePkId { get; set; }

    public string? TypeCode { get; set; }

    public string? TypeDescription { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
