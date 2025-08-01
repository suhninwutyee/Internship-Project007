using System;
using System.Collections.Generic;

namespace ProjectManagementSystem.ViewModels
{   
    public class DashboardViewModel
    {
        public int ProjectCount { get; set; }
        public int CompanyCount { get; set; }
        
        public int LanguageCount { get; set; }

        public List<RecentCompanyViewModel> RecentCompanies { get; set; }
    }
}
