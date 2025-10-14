using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectManagementSystem.Models;
using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.ViewModels
{
    public class ProjectIdeasViewModel
    {
        public List<Project> Projects { get; set; } = new();

        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 6;

        public int TotalProjects { get; set; }
        public string? SearchTerm { get; set; }

        // Safe division for pagination
        public int TotalPages => (int)Math.Ceiling((double)(TotalProjects) / PageSize);

        public List<SelectListItem> ProjectTypes { get; set; } = new();

        public int? SelectedProjectTypeId { get; set; }

        public List<SelectListItem> Languages { get; set; } = new();

        public int? SelectedLanguageId { get; set; }
        public List<Project> RelatedProjects { get; set; } = new();
    }
}
