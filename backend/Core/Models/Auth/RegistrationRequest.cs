using System.ComponentModel.DataAnnotations;

namespace Core.Models.Auth
{
    public class RegistrationRequest
    {
        [Required] public string Login { get; set; } = null!;
        [Required] public string Password { get; set; } = null!;
        [Required][EmailAddress] public string Email { get; set; } = null!;
    }
}
