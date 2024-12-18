using System.ComponentModel.DataAnnotations;

namespace Coinqueror.Shared.Models.DTOs
{
    public class LoginUserModelDTO
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
