using Template.Common.Requests.Users;
using Template.Common.Responses.Users;

namespace Template.Domain.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserResponse>?> GetAsync();
    Task<UserResponse?> GetByIdAsync(Guid id);
    Task<UserResponse?> UpdateAsync(UpdateUserRequest request);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> PromoteToAdminAsync(RoleChangeRequest request);
    Task<bool> DemoteToRegisteredCustomerAsync(RoleChangeRequest request);
}
