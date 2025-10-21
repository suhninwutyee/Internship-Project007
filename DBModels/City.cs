using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.DBModels;

public partial class City
{
    public int CityPkId { get; set; }

    public string? CityName { get; set; }

    public string? ImageFileName { get; set; }

    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
}
