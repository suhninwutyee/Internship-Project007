using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.DBModels
{
    public class ChooseRoleViewModel
    {
        public string RollNumber { get; set; } = "";

        [Required(ErrorMessage = "Role is required")]
        public string SelectedRole { get; set; } = "";
    }

}
