namespace ProjectManagementSystem.Models
{
    public class ProjectSubmitViewModel
    {
        public int Project_pkId { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }

        public List<ProjectMemberInfo> Members { get; set; } = new();

        public class ProjectMemberInfo
        {
            public int Student_pkId { get; set; }
            public string StudentName { get; set; }
            public string Email { get; set; }
        }
    }

}
