using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.DBModels
{
    public class NewMemberModel
    {
        [Required(ErrorMessage = "Student name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string StudentName { get; set; }

        [Required(ErrorMessage = "Roll number is required")]
        [StringLength(20, ErrorMessage = "Roll number cannot exceed 20 characters")]
        public string RollNumber { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public string Role { get; set; } = "Member"; // Default value
    }
}
