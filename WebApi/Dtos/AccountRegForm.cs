using System.ComponentModel.DataAnnotations;

namespace WebApi.Dtos;

public class AccountRegForm
{
    [Required(ErrorMessage = "Field is required")]
    [RegularExpression(@"^[A-Öa-ö]{2,}$", ErrorMessage = "Must contain at least 2 letters")]
    public string FirstName { get; set; } = null!;


    [Required(ErrorMessage = "Field is required")]
    [RegularExpression(@"^[A-Öa-ö]{2,}$", ErrorMessage = "Must contain at least 2 letters")]
    public string LastName { get; set; } = null!;


    [Required(ErrorMessage = "Field is required")]
    public DateOnly DateOfBirth { get; set; }


    public IFormFile? ProfileImage { get; set; }


    [RegularExpression(@"^((\+|00)[1-9]\d{0,3}|0)(\s|-)?[1-9]\d{1,2}(\s|-)?\d{3,4}(\s|-)?\d{3,4}$", 
        ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }


    [Required(ErrorMessage = "Field is required")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        ErrorMessage = "Invalid email address")]
    public string Email { get; set; } = null!;


    [Required(ErrorMessage = "Field is required")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
        ErrorMessage = "Password must be at least 8 characters and contain 1 upper and 1 lower case letter, 1 digit, 1 special character.")]
    public string Password { get; set; } = null!;


    [Required(ErrorMessage = "Field is required")]
    [Compare(nameof(Password), ErrorMessage = "Passwords does not match")]
    public string ConfirmPassword { get; set; } = null!;


    [Required(ErrorMessage = "Field is required")]
    [RegularExpression(@"^[A-Öa-ö]{2,}$", ErrorMessage = "Must contain at least 2 letters")]
    public string StreetAddress { get; set; } = null!;


    [Required(ErrorMessage = "Field is required")]
    [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9\s-]{2,9}$", ErrorMessage = "Invalid postal code")]
    public string PostalCode { get; set; } = null!;


    [Required(ErrorMessage = "Field is required")]
    [RegularExpression(@"^[A-Öa-ö]{2,}$", ErrorMessage = "Must contain at least 2 letters")]
    public string City { get; set; } = null!;


    [Required(ErrorMessage = "Field is required")]
    [Range(typeof(bool), "true", "true", ErrorMessage = "You need to accept terms and conditions")]
    public bool TermsAndConditions { get; set; }
}
