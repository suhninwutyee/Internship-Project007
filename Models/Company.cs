using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class Company
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Company_pkId { get; set; }

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public string CompanyName { get; set; }
        [Required(ErrorMessage = "Image is required")]
        [StringLength(200)]
        public string ImageFileName { get; set; }        

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public string Address { get; set; } = "";

        [Required(ErrorMessage = "Contact information is required")]
        [StringLength(50, ErrorMessage = "Contact information cannot exceed 50 characters")]
        public string Contact { get; set; } = "";

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; }
       
        [ForeignKey(nameof(City))]
        public int? City_pkId { get; set; }
        public virtual City City { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;



    }
}
