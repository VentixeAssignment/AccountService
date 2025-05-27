using System.ComponentModel.DataAnnotations;

namespace WebApi.Entities;

public class AccountEntity
{
    [Key]
    public string AccountId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string UserId { get; set; } = null!;

    [Required]
    [StringLength(20)]
    public string AccountType { get; set; } = null!;

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime Created { get; set; }
}
