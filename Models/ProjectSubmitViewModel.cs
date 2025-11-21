namespace ProjectManagementSystem.DBModels
{
    public class ProjectSubmitViewModel
    {
        public int ProjectPkId { get; set; }
        public string ProjectName { get; set; }
        public string Description { get; set; }

        public List<ProjectMemberInfo> Members { get; set; } = new();

        public class ProjectMemberInfo
        {
            public int StudentPkId { get; set; }
            public string StudentName { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
        }
    }

}
