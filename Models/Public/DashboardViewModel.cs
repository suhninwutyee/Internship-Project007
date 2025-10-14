using System.Collections.Generic;

namespace ProjectManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        public int ProjectCount { get; set; }
        public int ProjectTypeCount { get; set; }
        public int LanguageCount { get; set; }
        public List<string> ProjectTypeChartLabels { get; set; } = new List<string>();
        public List<int> ProjectTypeChartValues { get; set; } = new List<int>();

        // For bar chart
        public List<string> LanguageNames { get; set; }
        public List<int> LanguageCounts { get; set; }

        // For student dashboard inspiration section
        public List<ProjectIdea> PopularProjects { get; set; }
    }

    public class ProjectIdea
    {
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; } = "";
    }
}
