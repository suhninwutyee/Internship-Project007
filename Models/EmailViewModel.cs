// Models/EmailViewModel.cs
namespace ProjectManagementSystem.Models
{
    public class EmailViewModel
    {
       
        public Email NewEmail { get; set; }
        public List<Email> ExistingEmails { get; set; }
    }
}