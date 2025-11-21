using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.DBModels
{
    public class FrameworkCreateViewModel
    {
        [Required]
        public string FrameworkName { get; set; }

        [Required]
        [Display(Name = "Language")]
        public int SelectedLanguageId { get; set; }
        public IEnumerable<SelectListItem> Languages { get; set; }
    }
}