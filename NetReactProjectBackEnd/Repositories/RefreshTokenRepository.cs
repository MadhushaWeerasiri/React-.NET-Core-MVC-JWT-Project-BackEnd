using Dapper;
using NetReactProjectBackEnd.Data;
using NetReactProjectBackEnd.Models;

namespace NetReactProjectBackEnd.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly DapperContext _context;

    public RefreshTokenRepository(DapperContext context) => _context = context;

    public async Task SaveAsync(int userId, string token, DateTime expires)
    {
        using var con = _context.CreateConnection();
        await con.ExecuteAsync(
            "INSERT INTO RefreshTokens (UserId, Token, Expires, IsRevoked) VALUES (@UserId, @Token, @Expires, 0)",
            new { UserId = userId, Token = token, Expires = expires });
    }

    public async Task<RefreshToken?> GetValidTokenAsync(string token)
    {
        using var con = _context.CreateConnection();
        return await con.QueryFirstOrDefaultAsync<RefreshToken>(
            "SELECT * FROM RefreshTokens WHERE Token = @Token AND IsRevoked = 0 AND Expires > GETDATE()",
            new { Token = token });
    }

    public async Task RevokeAsync(string token)
    {
        using var con = _context.CreateConnection();
        await con.ExecuteAsync(
            "UPDATE RefreshTokens SET IsRevoked = 1 WHERE Token = @Token",
            new { Token = token });
    }
}