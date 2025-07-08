using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectManagementSystem.Models;
namespace ProjectManagementSystem.Models
{
    public class City
    {
        [Key]        
        public int City_pkId { get; set; }

        [Required]
        [StringLength(100)]
        public string CityName { get; set; }

        [StringLength(200)]
        public string ImageFileName { get; set; }  // e.g., /images/cities/yangon.jpg

        public ICollection<Company> Companies { get; set; }
    }
}
