using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NetReactProjectBackEnd.Models;
using NetReactProjectBackEnd.Repositories;

namespace NetReactProjectBackEnd.Services;

public class TokenService
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public TokenService(IConfiguration configuration, IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _configuration = configuration;
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username)
        };
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt.Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    public string GenerateRefreshToken() => Guid.NewGuid().ToString();

    public async Task SaveRefreshToken(int userId, string refreshToken)
    {
        await _refreshTokenRepository.SaveAsync(userId, refreshToken, DateTime.UtcNow.AddDays(7));
    }

    public async Task<(string AccessToken, string RefreshToken)?> Refresh(string refreshToken)
    {
        var rt = await _refreshTokenRepository.GetValidTockenAsync(refreshToken);
        if (rt == null) return null;
        
        var user = await _userRepository.GetByIdAsync(rt.UserId);
        if(user == null) return null;

        await _refreshTokenRepository.RevokeAsync(refreshToken);
        
        var newAccessToken = GenerateAccessToken(user);
        var newRefresh = GenerateRefreshToken();
        await SaveRefreshToken(user.Id, newRefresh);
        
        return (newAccessToken, newRefresh);
    }
        
}