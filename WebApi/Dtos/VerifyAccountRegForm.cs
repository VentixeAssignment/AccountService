namespace WebApi.Dtos;

public class VerifyAccountRegForm
{
    public string Email { get; set; } = null!;
    public string VerificationCode { get; set; } = null!;
}
