using System.ComponentModel.DataAnnotations;

namespace Template.Common.Requests.Users;

public class RoleChangeRequest
{
    [Required]
    public required Guid UserId { get; set; }
}
