using System.ComponentModel.DataAnnotations;

public class BulkEmailInputModel
{
    [Required]
    public List<EmailInputModel> Emails { get; set; }
}

public class EmailInputModel
{
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; }

    public string Class { get; set; } = "FinalYear";
}
