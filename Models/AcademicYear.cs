using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class AcademicYear
    {
        [Key]
        public int AcademicYear_pkId { get; set; }

        [Required]
        [MaxLength(20)]
        public string YearRange { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Student> Students { get; set; }
    }

}