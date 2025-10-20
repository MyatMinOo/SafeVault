// Models/UserDto.cs
using System.ComponentModel.DataAnnotations;
namespace SafeVault.Api.Models
{
    public class UserDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
    }
}
