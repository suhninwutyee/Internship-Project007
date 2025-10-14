using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class ProjectMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProjectMember_pkId { get; set; }
        
        [Required]
        [StringLength(150)]
        public string? Role { get; set; }

        [StringLength(100)]
        public string? RoleDescription { get; set; } // e.g. "Frontend", "Database"
        public int? Student_pkId { get; set; }
        public Student? Student { get; set; }

        public int? Project_pkId { get; set; }
        public Project? Project { get; set; }

        public bool IsDeleted { get; set; } = false;  
    }
}