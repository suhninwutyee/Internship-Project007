//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace ProjectManagementSystem.Models
//{
//    public class NRCType
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int NRCType_pkId { get; set; }

//        [Required(ErrorMessage = "Type code is required")]
//        [StringLength(5, ErrorMessage = "Type code cannot exceed 5 characters")]
//        public string TypeCode { get; set; }

//        [Required(ErrorMessage = "Type description is required")]
//        [StringLength(50, ErrorMessage = "Type description cannot exceed 50 characters")]
//        public string TypeDescription { get; set; }
//    }
//}