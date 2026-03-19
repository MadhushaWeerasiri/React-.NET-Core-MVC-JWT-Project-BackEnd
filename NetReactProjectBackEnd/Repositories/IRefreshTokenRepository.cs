using NetReactProjectBackEnd.Models;

namespace NetReactProjectBackEnd.Repositories;

public interface IRefreshTokenRepository
{
    Task SaveAsync(int userId, string token, DateTime expires);
    Task<RefreshToken?> GetValidTokenAsync(string token);
    Task RevokeAsync(string token);
}