using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class Company
{
    public int CompanyPkId { get; set; }

    public string? CompanyName { get; set; }

    public string? Incharge { get; set; }

    public string? Address { get; set; }

    public string? Contact { get; set; }

    public string? Description { get; set; }

    public int? CityPkId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public string? ImageFileName { get; set; }

    public virtual City? CityPk { get; set; } = null!;

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
}
