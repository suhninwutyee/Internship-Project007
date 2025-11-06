using System.ComponentModel.DataAnnotations;
//using ProjectManagementSystem.DBModels;
namespace ProjectManagementSystem.Models
{
    public class CompanyViewModel
    {
        public int Company_pkId { get; set; }
        public string CompanyName { get; set; }
    }

    public class CompanyNameModel
    {
        public int Company_pkId { get; set; }
        public int CityPKkId{ get; set; }

        [Required(ErrorMessage = "Company name is required")]
        [StringLength(100, ErrorMessage = "Company name cannot exceed 100 characters")]
        public string CompanyName { get; set; }
    }
}
