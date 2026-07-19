using AuctionPlatform.Domain.Entities;

namespace AuctionPlatform.Application.Common.Interfaces;

public interface IPaymentMethodRepository
{
    Task<List<PaymentMethod>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<PaymentMethod?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(PaymentMethod paymentMethod, CancellationToken ct = default);
    Task UpdateAsync(PaymentMethod paymentMethod, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
