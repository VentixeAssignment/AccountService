using System.ComponentModel.DataAnnotations;

namespace EmailFunction.Models;

public class SaveVerificationCodeModel
{
    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string VerificationCode { get; set; } = null!;
    public TimeSpan ValidFor { get; set; }
}
