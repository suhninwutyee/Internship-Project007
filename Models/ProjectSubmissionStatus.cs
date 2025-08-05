namespace ProjectManagementSystem.Models
{
    public class ProjectSubmissionStatus
    {
        public int TotalProjects { get; set; }
        public int DraftProjects { get; set; }
        public int PendingProjects { get; set; }
        public int ApprovedProjects { get; set; }
        public int RejectedProjects { get; set; }
        public int RevisionRequired { get; set; }

    }
}
