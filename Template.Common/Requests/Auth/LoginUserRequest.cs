using System.ComponentModel.DataAnnotations;

namespace Template.Common.Requests.Auth;

public class LoginUserRequest
{
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}
