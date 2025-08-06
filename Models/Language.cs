using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.Models
{
    public class Language
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int Language_pkId { get; set; }

        [Required(ErrorMessage = "Language Name is required")]
        public string? LanguageName { get; set; } = "";
        [ForeignKey(nameof(ProjectType_pkId))]
        public int? ProjectType_pkId { get; set; }
        public ProjectType? ProjectType { get; set; }
        public virtual ICollection<Project>? Projects { get; set; }
        public virtual ICollection<Framework>? Frameworks { get; set; }
    }
}