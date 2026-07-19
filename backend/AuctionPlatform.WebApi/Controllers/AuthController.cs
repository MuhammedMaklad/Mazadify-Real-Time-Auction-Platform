using AuctionPlatform.Application.Auth.DTOs;
using AuctionPlatform.Application.Auth.Interfaces;
using AuctionPlatform.Application.Auth.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionPlatform.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly RegisterRequestValidator _registerValidator;
    private readonly LoginRequestValidator _loginValidator;

    private const string RefreshTokenCookieName = "refreshToken";
    private static readonly CookieOptions RefreshTokenCookieOptions = new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        MaxAge = TimeSpan.FromDays(7)
    };

    public AuthController(
        IAuthService authService,
        RegisterRequestValidator registerValidator,
        LoginRequestValidator loginValidator)
    {
        _authService = authService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var validation = await _registerValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var result = await _authService.RegisterAsync(request, ct);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var validation = await _loginValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return BadRequest(validation.Errors);

        var (auth, refreshToken) = await _authService.LoginAsync(request, ct);

        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, RefreshTokenCookieOptions);

        return Ok(auth);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { error = "Refresh token is missing." });

        var (auth, newRefreshToken) = await _authService.RefreshAsync(refreshToken, ct);

        Response.Cookies.Append(RefreshTokenCookieName, newRefreshToken, RefreshTokenCookieOptions);

        return Ok(auth);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var refreshToken = Request.Cookies[RefreshTokenCookieName];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _authService.RevokeAsync(refreshToken, ct);
        }

        Response.Cookies.Delete(RefreshTokenCookieName);
        return NoContent();
    }
}
