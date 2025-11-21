//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace ProjectManagementSystem.Models
//{
//    public class StudentDepartment
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int Department_pkID { get; set; }

//        [Required(ErrorMessage = "Department name is required")]
//        [StringLength(100, ErrorMessage = "Department name cannot exceed 100 characters")]
//        public string DepartmentName { get; set; }

//        public ICollection<Student> Students { get; set; }
//    }
//}