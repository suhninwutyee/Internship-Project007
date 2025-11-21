//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace ProjectManagementSystem.Models
//{
//    public class Framework
//    {
//        [Key]
//        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//        public int Framework_pkId { get; set; }

//        [Required(ErrorMessage = "Framework name is required")]
//        [StringLength(50, ErrorMessage = "Framework name cannot exceed 50 characters")]
//        public string FrameworkName { get; set; }

//        [ForeignKey(nameof(Language))]
//        public int Language_pkId { get; set; }
//        public virtual Language Language { get; set; }

//        public virtual ICollection<Project> Projects { get; set; }
//    }
//}