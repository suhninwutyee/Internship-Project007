using X.PagedList;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.DBModels
{
    public class StudentDashboardViewModel
    {
        public Student Student { get; set; }
        public List<Project> Projects { get; set; }
        public List<ProjectMember> TeamMembers { get; set; }
        public ProjectSubmissionStatus SubmissionStatus { get; set; }

        public List<Project> LeaderProjects { get; set; }

        public List<NotificationViewModel> Notifications { get; set; } = new List<NotificationViewModel>();

        public bool IsMember { get; set; }
        public string? MemberProjectName { get; set; }

        public string? MemberResponsibility { get; set; }
        public string? LeaderName { get; set; }
        public string? LeaderEmail { get; set; }
    }

}
