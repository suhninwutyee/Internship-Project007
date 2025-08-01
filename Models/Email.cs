//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//using ProjectManagementSystem;
//using ProjectManagementSystem.Models;
//namespace ProjectManagementSystem.Models
//{
//    public class Email
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int Email_PkId { get; set; }

//        [Required(ErrorMessage = "Email address is required")]
//        [EmailAddress(ErrorMessage = "Invalid email format")]
//        public string EmailAddress { get; set; } = "";

//        [Required(ErrorMessage = "Roll number is required")]
//        public string RollNumber { get; set; } = "";

//        [Required(ErrorMessage = "Class is required")]
//        public string Class { get; set; } = "Final Year"; // Auto-set default value

//        public string AcademicYear { get; set; }

//        public bool IsDeleted { get; set; } = false; // Default value

//        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now; // Auto-set

//    }

//}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using ProjectManagementSystem;
using ProjectManagementSystem.Models;
namespace ProjectManagementSystem.Models
{
    public class Email
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Email_PkId { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [MaxLength(50)]
        public string EmailAddress { get; set; } = "";

        [Required(ErrorMessage = "Roll number is required")]
        [MaxLength(50)]
        public string RollNumber { get; set; } = "";

        [Required(ErrorMessage = "Class is required")]
        [MaxLength(50)]
        public string Class { get; set; } = "Final Year"; // Auto-set default value

        [ForeignKey(nameof(AcademicYear))]
        public int? AcademicYear_pkId { get; set; }
        public virtual AcademicYear? AcademicYear { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now; // Auto-set

        public virtual ICollection<Student>? Students { get; set; }

    }

}