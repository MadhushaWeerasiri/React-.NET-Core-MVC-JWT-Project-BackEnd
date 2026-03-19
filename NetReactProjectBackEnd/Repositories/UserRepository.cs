using Dapper;
using NetReactProjectBackEnd.Data;
using NetReactProjectBackEnd.Models;

namespace NetReactProjectBackEnd.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DapperContext _context;

    public UserRepository(DapperContext context) => _context = context;

    public async Task<User?> GetByUserNameAsync(string username)
    {
        using var con = _context.CreateConnection();
        return await con.QueryFirstOrDefaultAsync<User>(
            "SELECT * FROM Users WHERE Username = @Username", new { Username = username });
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var con = _context.CreateConnection();
        var result = await con.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM users WHERE Id = @Id", new { Id = id });
        return new User
        {
            Id = result?.Id,
            Username = result?.Username,
            PasswordHash = result?.PasswordHash,
        };
    }
}