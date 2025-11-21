//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace ProjectManagementSystem.Models
//{
//    public class AuditLog
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int Log_pkId { get; set; }

//        [Required(ErrorMessage = "Student name is required")]
//        [StringLength(100, ErrorMessage = "Student name cannot exceed 100 characters")]
//        public string StudentName { get; set; }

//        [Required(ErrorMessage = "Student ID is required")]
//        public int Student_pkId { get; set; }

//        public Student Student { get; set; }

//        [Required(ErrorMessage = "Action is required")]
//        [StringLength(50, ErrorMessage = "Action cannot exceed 50 characters")]
//        public string Action { get; set; }

//        [Required(ErrorMessage = "Performer name is required")]
//        [StringLength(100, ErrorMessage = "Performer name cannot exceed 100 characters")]
//        public string PerformedBy { get; set; }

//        public DateTime PerformedAt { get; set; } = DateTime.Now;
//    }
//}