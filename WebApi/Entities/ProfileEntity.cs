using System.ComponentModel.DataAnnotations;

namespace WebApi.Entities
{
    public class ProfileEntity
    {
        private string _profileImageUrl = "/Images/standard-user-avatar.jpg";
        
        
        [Key]
        public string ProfileId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; } = null!;

        [Required]
        [StringLength(30)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(30)]
        public string LastName { get; set; } = null!;

        [Required]
        [DataType(DataType.Date)]
        public DateOnly DateOfBirth { get; set; }
                
        [Required]
        [StringLength(500)]
        public string ProfileImageUrl
        {
            get => _profileImageUrl;
            set => _profileImageUrl = string.IsNullOrWhiteSpace(value)
                ? "/Images/standard-user-avatar.jpg"
                : value;
        }

        [Required]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(50)]
        public string StreetAddress { get; set; } = null!;

        [Required]
        [StringLength(10)]
        public string PostalCode { get; set; } = null!;

        [Required]
        [StringLength(30)]
        public string City { get; set; } = null!;
    }
}
