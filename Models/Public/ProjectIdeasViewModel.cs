using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectManagementSystem.Models;

namespace ProjectManagementSystem.ViewModels
{
    public class ProjectIdeasViewModel
    {
        public List<Project> Projects { get; set; } = new();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalProjects { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalProjects / PageSize);

        public List<SelectListItem> ProjectTypes { get; set; } = new();
        public int? SelectedProjectTypeId { get; set; }
    }
}
