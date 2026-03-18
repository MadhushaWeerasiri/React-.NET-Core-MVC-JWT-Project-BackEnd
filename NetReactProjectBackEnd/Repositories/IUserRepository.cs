using NetReactProjectBackEnd.Models;

namespace NetReactProjectBackEnd.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUserNameAsync(string username);
    Task<User?> GetByIdAsync(int id);
}