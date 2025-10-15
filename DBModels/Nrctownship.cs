using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class Nrctownship
{
    public int NrcPkId { get; set; }

    public string? TownshipCodeM { get; set; }

    public string? TownshipCodeE { get; set; }

    public string? TownshipName { get; set; }

    public string? RegionCodeE { get; set; }

    public string? RegionCodeM { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
