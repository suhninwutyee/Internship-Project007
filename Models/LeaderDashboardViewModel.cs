namespace ProjectManagementSystem.Models
{
    public class LeaderDashboardViewModel
    {
        public Student Student { get; set; }
        public Project Project { get; set; }
        public List<ProjectMember> Members { get; set; } = new();
        public bool CanSubmitProject { get; internal set; }
    }
}
