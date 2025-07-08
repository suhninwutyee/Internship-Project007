using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class ProjectType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProjectType_pkId { get; set; }

        [Required(ErrorMessage = "Type name is required")]
        [StringLength(50, ErrorMessage = "Type name cannot exceed 50 characters")]
        public string TypeName { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
    }
}