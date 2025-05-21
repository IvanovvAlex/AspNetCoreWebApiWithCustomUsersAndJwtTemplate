using Template.Common.Requests.Users;
using Template.Common.Responses.Users;
using Template.Core.Exceptions;
using Template.Core.StaticClasses;
using Template.Data.Entities;
using Template.Data.Interfaces;
using Template.Domain.Interfaces;

namespace Template.Domain.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<IEnumerable<UserResponse>?> GetAsync()
    {
        IEnumerable<User> users = await userRepository.GetAllAsync();

        return users.Select(user => new UserResponse()
        {
            Id = user.Id,
            Email = user.Email,
            Names = user.Names,
            Phone = user.Phone,
            Role = user.Role,

        });
    }
    public async Task<UserResponse?> GetByIdAsync(Guid id)
    {
        User? user = await userRepository.GetByIdAsync(id) ?? throw new AppException("User not found.").SetStatusCode(404);
        return new()
        {
            Id = user.Id,
            Email = user.Email,
            Names = user.Names,
            Phone = user.Phone,
            Role = user.Role,
        };
    }

    public async Task<UserResponse?> UpdateAsync(UpdateUserRequest request)
    {
        User userBeforeUpdate = (await userRepository.GetByIdAsync(request.Id))!;

        User? updatedUser = new()
        {
            Id = request.Id,
            Email = request.Email,
            Names = request.Names,
            Phone = request.Phone,
            PasswordHash = userBeforeUpdate.PasswordHash,
        };

        updatedUser = await userRepository.UpdateAsync(updatedUser);

        return new()
        {
            Id = updatedUser!.Id,
            Email = updatedUser.Email,
            Names = updatedUser.Names,
            Phone = updatedUser.Phone,
            Role = updatedUser.Role,
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        UserResponse user = (await GetByIdAsync(id))!;

        if (!await userRepository.DeleteAsync(user.Id))
        {
            return false;
        }

        return true;
    }

    public async Task<bool> PromoteToAdminAsync(RoleChangeRequest request)
    {
        return await ChangeRoleAsync(request, Roles.Admin);
    }

    public async Task<bool> DemoteToRegisteredCustomerAsync(RoleChangeRequest request)
    {
        return await ChangeRoleAsync(request, Roles.Admin);
    }

    private async Task<bool> ChangeRoleAsync(RoleChangeRequest request, string toRole)
    {
        User userBeforeUpdate = (await userRepository.GetByIdAsync(request.UserId))! ?? throw new AppException("User not found.").SetStatusCode(404);

        User? updatedUser = new()
        {
            Id = request.UserId,
            Email = userBeforeUpdate.Email,
            Names = userBeforeUpdate.Names,
            Phone = userBeforeUpdate.Phone,
            PasswordHash = userBeforeUpdate.PasswordHash,
            Role = toRole,
        };

        updatedUser = await userRepository.UpdateAsync(updatedUser);

        return true;
    }
}
