using NetReactProjectBackEnd.Models;

namespace NetReactProjectBackEnd.Repositories;

public interface IRefreshTokenRepository
{
    Task SaveAsync(int userId, string token, DateTime expires);
    Task <RefreshToken?> GetValidTockenAsync(string token);
    Task RevokeAsync(string token);
}