using AuctionPlatform.Application.PaymentMethods.DTOs;

namespace AuctionPlatform.Application.PaymentMethods.Interfaces;

public interface IPaymentMethodService
{
    Task<List<PaymentMethodDto>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<PaymentMethodDto> AddAsync(Guid userId, AddPaymentMethodRequest request, CancellationToken ct = default);
    Task SetDefaultAsync(Guid userId, Guid paymentMethodId, CancellationToken ct = default);
    Task DeactivateAsync(Guid userId, Guid paymentMethodId, CancellationToken ct = default);
}
