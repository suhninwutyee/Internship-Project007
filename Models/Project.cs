using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class Project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Project_pkId { get; set; }

        [Required(ErrorMessage = "Project name is required")]
        [StringLength(200, ErrorMessage = "Project name cannot exceed 200 characters")]
        public string? ProjectName { get; set; } = "";

        [Required(ErrorMessage = "SupervisorName is required")]
        [StringLength(1000, ErrorMessage = "SupervisorName cannot exceed 1000 characters")]
        public string? SupervisorName { get; set; } = "";

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; } = "";

        [ForeignKey(nameof(ProjectType))]
        public int? ProjectType_pkId { get; set; }

        [ForeignKey(nameof(Language))]
        public int? Language_pkId { get; set; }

        [ForeignKey(nameof(Framework))]
        public int? Framework_pkId { get; set; }
        [ForeignKey(nameof(Company))]
        public int? Company_pkId { get; set; }      

        public DateTime ProjectSubmittedDate { get; set; } = DateTime.Now;

        //[StringLength(500, ErrorMessage = "Remark cannot exceed 500 characters")]
        //public string? Remark { get; set; } = "";
        [Required]
        [StringLength(50)]
        public string? Status { get; set; } = "Pending";
        // Default to Pending

        public bool? IsDeleted { get; set; } = false;
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Creator name is required")]
        [StringLength(100, ErrorMessage = "Creator name cannot exceed 100 characters")]
        public string? CreatedBy { get; set; } = "";
        public virtual Company Company { get; set; }
        public virtual ProjectType? ProjectType { get; set; }
        public virtual Language? Language { get; set; }
        public virtual Framework? Framework { get; set; }
       
        public virtual ICollection<ProjectFile>? Files { get; set; }
        public virtual ICollection<ProjectMember>? ProjectMembers { get; set; }
        
    }
}