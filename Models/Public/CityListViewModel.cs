using ProjectManagementSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;
using System.Collections.Generic;

namespace ProjectManagementSystem.ViewModels
{
    public class CityListViewModel
    {
        public IPagedList<City> Cities { get; set; }
        public int? SelectedCityId { get; set; }
        public IEnumerable<SelectListItem> CityList { get; set; }
        public int TotalCities { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages => Cities?.PageCount ?? 1;
        
    }
}
