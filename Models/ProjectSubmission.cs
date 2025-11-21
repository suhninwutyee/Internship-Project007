using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementSystem.DBModels
{
    public class ProjectSubmission
    {
        public int ProjectSubmission_pkId { get; set; }
        public int Project_pkId { get; set; }
        public int SubmittedBy { get; set; } // Student_pkId
        public DateTime SubmissionDate { get; set; }
        public string Status { get; set; } // Pending, Approved, Rejected
        public string Notes { get; set; }

        [ForeignKey("Project_pkId")]
        public Project Project { get; set; }

        [ForeignKey("SubmittedBy")]
        public Student Student { get; set; }
    }
}
