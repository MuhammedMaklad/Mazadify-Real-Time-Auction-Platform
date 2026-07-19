using AuctionPlatform.Application.Auth.DTOs;
using AuctionPlatform.Application.Auth.Interfaces;
using AuctionPlatform.Application.Common.Exceptions;
using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuctionPlatform.Application.Auth.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthService(
        UserManager<User> userManager,
        IJwtService jwtService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            throw new InvalidOperationException("A user with this email already exists.");

        var existingByUsername = await _userManager.FindByNameAsync(request.Username);
        if (existingByUsername is not null)
            throw new InvalidOperationException("A user with this username already exists.");

        var user = new User
        {
            UserName = request.Username,
            Email = request.Email,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Registration failed: {errors}");
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);

        return BuildAuthResponse(user, accessToken, roles);
    }

    public async Task<(AuthResponse Auth, string RefreshToken)> LoginAsync(
        LoginRequest request, CancellationToken ct = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedException("Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedException("Account is deactivated.");

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(refreshToken, ct);
        await _refreshTokenRepository.SaveChangesAsync(ct);

        return (BuildAuthResponse(user, accessToken, roles), refreshTokenValue);
    }

    public async Task<(AuthResponse Auth, string NewRefreshToken)> RefreshAsync(
        string refreshToken, CancellationToken ct = default)
    {
        var existing = await _refreshTokenRepository.GetActiveByTokenAsync(refreshToken, ct);
        if (existing is null)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        var user = await _userManager.FindByIdAsync(existing.UserId.ToString());
        if (user is null || !user.IsActive)
            throw new UnauthorizedException("User not found or deactivated.");

        var newRefreshTokenValue = _jwtService.GenerateRefreshToken();
        await _refreshTokenRepository.RevokeAsync(existing, newRefreshTokenValue, ct);

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken, ct);
        await _refreshTokenRepository.SaveChangesAsync(ct);

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(user, roles);

        return (BuildAuthResponse(user, accessToken, roles), newRefreshTokenValue);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken ct = default)
    {
        var existing = await _refreshTokenRepository.GetActiveByTokenAsync(refreshToken, ct);
        if (existing is null)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        await _refreshTokenRepository.RevokeAsync(existing, ct: ct);
        await _refreshTokenRepository.SaveChangesAsync(ct);
    }

    private static AuthResponse BuildAuthResponse(User user, string accessToken, IList<string> roles)
    {
        return new AuthResponse
        {
            AccessToken = accessToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(15),
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Roles = roles
            }
        };
    }
}
