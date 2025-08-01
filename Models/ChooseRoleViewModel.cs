using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class ChooseRoleViewModel
    {
        public string RollNumber { get; set; } = "";

        [Required(ErrorMessage = "Role is required")]
        public string SelectedRole { get; set; } = "";
    }

}
