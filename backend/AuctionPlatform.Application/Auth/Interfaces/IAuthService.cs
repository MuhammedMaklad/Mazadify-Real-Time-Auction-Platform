using AuctionPlatform.Application.Auth.DTOs;

namespace AuctionPlatform.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<(AuthResponse Auth, string RefreshToken)> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<(AuthResponse Auth, string NewRefreshToken)> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task RevokeAsync(string refreshToken, CancellationToken ct = default);
}
