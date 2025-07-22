using X.PagedList;

namespace ProjectManagementSystem.Models
{
    public class StudentDashboardViewModel
    {
        public IPagedList<Student> Students { get; set; }
        public IPagedList<Project> Projects { get; set; }
        public Student? LoggedInStudent { get; set; }  

    }
}
