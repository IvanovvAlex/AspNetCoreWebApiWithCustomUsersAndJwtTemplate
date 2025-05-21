using Template.Data.Entities;

namespace Template.Data.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<bool> IsEmailAlreadyUsed(string email);
}