using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class NRCTownship
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NRC_pkId { get; set; }

        public string? RegionCode_E { get; set; }

        public string? RegionCode_M { get; set; }

        public string? TownshipCode_M { get; set; }

        public string? TownshipCode_E { get; set; }

        public string? TownshipName { get; set; }
    }
}
