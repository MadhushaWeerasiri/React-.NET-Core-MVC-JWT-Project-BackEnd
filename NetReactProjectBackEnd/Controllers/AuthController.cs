using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetReactProjectBackEnd.Models;
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

    public AuthController(
        IUserRepository userRepository,
        TokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    // COMMON COOKIE OPTIONS (reuse everywhere)
    private CookieOptions GetCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // false for localhost (HTTP)
            SameSite = SameSiteMode.Lax, // allows cross-port (5173 -> 5019)
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/"
        };
    }
    
    // REGISTER
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Username and password is required");
        var existingUser = await _userRepository.GetByUserNameAsync(dto.Username);
        if (existingUser != null)
            return BadRequest("Username already exists");
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var user = new User
        {
            Username = dto.Username,
            PasswordHash = passwordHash,
        };
        
        var userId = await _userRepository.RegisterAsync(user);
        user.Id = userId;
        
        return Ok( new { message = "registered successfully" });
    }

    // LOGIN
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userRepository.GetByUserNameAsync(dto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid username or password");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        await _tokenService.SaveRefreshToken(user.Id, refreshToken);

        Response.Cookies.Append("refreshToken", refreshToken, GetCookieOptions());

        return Ok(new { access_token = accessToken });
    }

    // REFRESH TOKEN
    [HttpGet("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized("Refresh token missing");

        var result = await _tokenService.Refresh(refreshToken);

        if (!result.HasValue)
            return Unauthorized("Invalid or expired refresh token");

        // Rotate refresh token
        Response.Cookies.Append("refreshToken", result.Value.RefreshToken, GetCookieOptions());

        return Ok(new { access_token = result.Value.AccessToken });
    }

    // LOGOUT
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _refreshTokenRepository.RevokeAsync(refreshToken);
        }

        // Properly delete cookie (must match options!)
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });

        return Ok();
    }

    // GET CURRENT USER
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return NotFound();

        return Ok(new
        {
            id = user.Id,
            username = user.Username
        });
    }

    // DTOs
    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    
    public class RegisterDto
        {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        }
}