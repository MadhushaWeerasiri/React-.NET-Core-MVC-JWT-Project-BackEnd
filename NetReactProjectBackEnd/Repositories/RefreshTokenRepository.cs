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
            "INSERT INTO RefrshTockens (UserId, Token, Expires, IsRevokec) VALUES (@userId, @token, @expires, 0)",
            new {UserId = userId, Token = token, Expires = expires} );
    }

    public async Task<RefreshToken?> GetValidTockenAsync(string token)
    {
        using var con = _context.CreateConnection();
        return await con.QueryFirstOrDefaultAsync<RefreshToken>(
            "SELECT * FROM RefreshTokens WHERE Token = @token AND IsRevoked = 0 AND Expires > GETDATE()",
            new {Token = token});
    }

    public async Task RevokeAsync(string token)
    {
        using var con = _context.CreateConnection();
        await con.ExecuteAsync(
            "UPDATE RefreshTokens SET IsRevoked = 1 WHERE Token = @token",
            new { Token = token });
    }
    
}