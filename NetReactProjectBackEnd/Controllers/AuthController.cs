using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetReactProjectBackEnd.Repositories;
using NetReactProjectBackEnd.Services;

namespace NetReactProjectBackEnd.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly TokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthController(IUserRepository userRepository, TokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userRepository.GetByUserNameAsync(dto.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized();
        
        var access = _tokenService.GenerateAccessToken(user);
        var refresh = _tokenService.GenerateRefreshToken();
        await _tokenService.SaveRefreshToken(user.Id, refresh);
        
        return Ok(new {access_token = access, refresh_token = refresh});
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshDto dto)
    {
        var result = await _tokenService.Refresh(dto.RefreshToken);
        return result.HasValue
            ? Ok(new { accessToken = result.Value.AccessToken, refreshToken = result.Value.RefreshToken })
            : Unauthorized();
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshDto dto)
    {
        await _refreshTokenRepository.RevokeAsync(dto.RefreshToken);
        return Ok();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _userRepository.GetByIdAsync(userId);
        return Ok(new { UserId = user?.Id, Username = user?.Username });
    }
    
    public class LoginDto { public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class RefreshDto { public string RefreshToken { get; set; } = string.Empty; }
}