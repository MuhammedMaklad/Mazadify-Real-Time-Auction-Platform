using AuctionPlatform.Domain.Entities;

namespace AuctionPlatform.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user, IList<string> roles);
    string GenerateRefreshToken();
}
