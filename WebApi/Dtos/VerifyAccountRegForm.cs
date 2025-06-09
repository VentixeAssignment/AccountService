namespace WebApi.Dtos;

public class VerifyAccountRegForm
{
    public string Email { get; set; } = null!;
    public string Code { get; set; } = null!;
}
