using System.ComponentModel.DataAnnotations;

namespace ProjectManagementSystem.Models
{
    public class AddMemberViewModel
    {
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Roll Number is required")]
        public string RollNumber { get; set; }

        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string EmailAddress { get; set; }

        public string ProjectName { get; set; }

        public ProjectType ProjectType { get; set; }
        public Language Language { get; set; }
        public Framework Framework { get; set; }

    }
}
