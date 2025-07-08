using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
<<<<<<< Updated upstream
using ProjectManagementSystem;
using ProjectManagementSystem.Models;
public class Email
{
    [Key]
    public int Email_PkId { get; set; }
    public string EmailAddress { get; set; }
    public string Class { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedDate { get; set; }

    [Required]
    public string RollNumber { get; set; }


=======

namespace ProjectManagementSystem.Models
{
    public class Email
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Email_PkId { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string EmailAddress { get; set; } = "";

        [Required(ErrorMessage = "Roll number is required")]
        public string RollNumber { get; set; } = "";

        [Required(ErrorMessage = "Class is required")]
        public string Class { get; set; } = "Final Year"; // Auto-set default value

        public bool IsDeleted { get; set; } = false; // Default value

        public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.Now; // Auto-set

      
    }
>>>>>>> Stashed changes
}