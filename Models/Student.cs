//using System.ComponentModel.DataAnnotations.Schema;
//using System.ComponentModel.DataAnnotations;

//namespace ProjectManagementSystem.Models
//{
//    public class Student
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int Student_pkId { get; set; }

//        [Required(ErrorMessage = "Student Name is required")]
//        public string StudentName { get; set; } = "";

//        [Required(ErrorMessage = "Roll Number is required")]
//        public string RollNumber { get; set; } = "";


//        [Required(ErrorMessage = "Department is required")]
//        [ForeignKey(nameof(StudentDepartment))]
//        public int? Department_pkID { get; set; }

//        public virtual StudentDepartment? StudentDepartment { get; set; }

//        [Required(ErrorMessage = "Email is required")]
//        [EmailAddress(ErrorMessage = "Invalid email address")]
//        public string Email { get; set; } = "";

//        [Required(ErrorMessage = "Phone Number is required")]
//        [Phone(ErrorMessage = "Invalid phone number")]
//        public string PhoneNumber { get; set; } = "";

//        [Required(ErrorMessage = "NRC Type is required")]
//        [ForeignKey(nameof(NRCType))]
//        public int NRCType_pkId { get; set; }

//        [Required(ErrorMessage = "NRC Township is required")]
//        [ForeignKey(nameof(NRCTownship))]
//        public int? NRC_pkId { get; set; }

//        [Required(ErrorMessage = "NRC Number is required")]
//        [Range(1, 999999, ErrorMessage = "NRC Number must be between 1 and 999999")]
//        public int NRCNumber { get; set; }

//        public bool IsDeleted { get; set; } = false;
//        public DateTime CreatedDate { get; set; } = DateTime.Now;

//        [Required(ErrorMessage = "Created By is required")]
//        public string CreatedBy { get; set; } = "";

//        public virtual NRCTownship? NRCTownship { get; set; }
//        public virtual NRCType? NRCType { get; set; }
//        public ICollection<Project> Projects { get; set; }
//        public ICollection<ProjectMember> ProjectMembers { get; set; }

//        [Required(ErrorMessage = "AcademicYear is required")]
//        [ForeignKey(nameof(AcademicYear))]
//        public int AcademicYear_pkId { get; set; }
//        public AcademicYear? AcademicYear { get; set; }
//    }
//}
//using System.ComponentModel.DataAnnotations.Schema;
//using System.ComponentModel.DataAnnotations;

//namespace ProjectManagementSystem.Models
//{
//    public class Student
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int Student_pkId { get; set; }

//        [Required(ErrorMessage = "Department is required")]
//        [ForeignKey(nameof(StudentDepartment))]
//        public int? Department_pkID { get; set; }

//        [Required(ErrorMessage = "Student Name is required")]
//        [MaxLength(50)] 
//        public string StudentName { get; set; } = "";
//        public virtual StudentDepartment? StudentDepartment { get; set; }

//        [ForeignKey(nameof(Email))]
//        public int? Email_PkId { get; set; }
//        public virtual Email? Email { get; set; }

//        [Required(ErrorMessage = "Phone Number is required")]
//        [Phone(ErrorMessage = "Invalid phone number")]
//        [MaxLength(50)] 
//        public string PhoneNumber { get; set; } = "";

//        [Required(ErrorMessage = "NRC Type is required")]
//        [ForeignKey(nameof(NRCType))]
//        public int NRCType_pkId { get; set; }

//        [Required(ErrorMessage = "NRC Township is required")]
//        [ForeignKey(nameof(NRCTownship))]
//        public int? NRC_pkId { get; set; }

//        [Required(ErrorMessage = "NRC Number is required")]
//        [Range(1, 999999, ErrorMessage = "NRC Number must be between 1 and 999999")]
//        public int NRCNumber { get; set; }

//        public bool IsDeleted { get; set; } = false;
//        public DateTime CreatedDate { get; set; } = DateTime.Now;

//        [Required(ErrorMessage = "Created By is required")]
//        [MaxLength(50)]
//        public string CreatedBy { get; set; } = "";

//        public virtual NRCTownship? NRCTownship { get; set; }
//        public virtual NRCType? NRCType { get; set; }
//        public ICollection<Project> Projects { get; set; }
//        public ICollection<ProjectMember> ProjectMembers { get; set; }

//        [Required(ErrorMessage = "AcademicYear is required")]
//        [ForeignKey(nameof(AcademicYear))]
//        public int AcademicYear_pkId { get; set; }
//        public virtual AcademicYear? AcademicYear { get; set; }
//    }
//}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class Student
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Student_pkId { get; set; }

        [Required(ErrorMessage = "Student Name is required")]
        [MaxLength(50)]
        public string StudentName { get; set; } = "";

        [ForeignKey(nameof(StudentDepartment))]
        public int? Department_pkID { get; set; }
        public virtual StudentDepartment? StudentDepartment { get; set; }

        [ForeignKey(nameof(Email))]
        public int? Email_PkId { get; set; }
        public virtual Email? Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [MaxLength(50)]
        public string PhoneNumber { get; set; } = "";

        [Required(ErrorMessage = "NRC Type is required")]
        [ForeignKey(nameof(NRCType))]
        public int NRCType_pkId { get; set; }
        public virtual NRCType? NRCType { get; set; }

        [Required(ErrorMessage = "NRC Township is required")]
        [ForeignKey(nameof(NRCTownship))]
        public int? NRC_pkId { get; set; }
        public virtual NRCTownship? NRCTownship { get; set; }

        [Required(ErrorMessage = "NRC Number is required")]
        [Range(1, 999999, ErrorMessage = "NRC Number must be between 1 and 999999")]
        public int NRCNumber { get; set; }

        [Required(ErrorMessage = "AcademicYear is required")]
        [ForeignKey(nameof(AcademicYear))]
        public int AcademicYear_pkId { get; set; }
        public virtual AcademicYear? AcademicYear { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Created By is required")]
        [MaxLength(50)]
        public string CreatedBy { get; set; } = "";

        public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();

        // One student can submit one project
        public virtual Project? SubmittedProject { get; set; }
    }
}
