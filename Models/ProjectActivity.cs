using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.DBModels
{
    public class ProjectActivity
    {
        public int ProjectActivity_pkId { get; set; }
        public int Project_pkId { get; set; }
        public int UserId { get; set; } // Student_pkId
        public string ActivityType { get; set; }
        public string Description { get; set; }
        public DateTime ActivityDate { get; set; }

        [ForeignKey("Project_pkId")]
        public Project Project { get; set; }

        [ForeignKey("UserId")]
        public Student Student { get; set; }
    }
}
