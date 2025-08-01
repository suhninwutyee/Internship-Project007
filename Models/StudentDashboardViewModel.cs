using X.PagedList;

namespace ProjectManagementSystem.Models
{
    public class StudentDashboardViewModel
    {
        public Student Student { get; set; }
        public List<Project> Projects { get; set; }
        public List<ProjectMember> TeamMembers { get; set; }
        public ProjectSubmissionStatus SubmissionStatus { get; set; }

        public List<Project> LeaderProjects { get; set; }  
    }

}
