using System.ComponentModel.DataAnnotations;

namespace Core.Models.Auth
{
    public class PersonalRegistrationRequest
    {
        [Required] public string Login { get; set; } = null!;
        [Required] public string Password { get; set; } = null!;
        [Required][EmailAddress] public string Email { get; set; } = null!;
        [Required] public string FIO { get; set; } = null!;
        public string? Position { get; set; }
        public string? Description { get; set; }
    }
}
