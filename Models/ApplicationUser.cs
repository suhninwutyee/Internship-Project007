using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.DBModels
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; } = "";
        public bool IsUsingDefaultPassword { get; set; } = true;
    }
}
