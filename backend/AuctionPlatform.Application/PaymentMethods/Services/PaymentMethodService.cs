using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Application.PaymentMethods.DTOs;
using AuctionPlatform.Application.PaymentMethods.Interfaces;
using AuctionPlatform.Domain.Entities;
using AuctionPlatform.Domain.ValueTypes;
using AutoMapper;

namespace AuctionPlatform.Application.PaymentMethods.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IPaymentMethodRepository _repository;
    private readonly IMapper _mapper;

    public PaymentMethodService(IPaymentMethodRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<PaymentMethodDto>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var methods = await _repository.GetByUserIdAsync(userId, ct);
        return _mapper.Map<List<PaymentMethodDto>>(methods);
    }

    public async Task<PaymentMethodDto> AddAsync(
        Guid userId, AddPaymentMethodRequest request, CancellationToken ct = default)
    {
        var existing = await _repository.GetByUserIdAsync(userId, ct);

        var isFirstMethod = existing.Count == 0;
        var shouldBeDefault = request.IsDefault || isFirstMethod;

        if (shouldBeDefault)
        {
            foreach (var m in existing.Where(m => m.IsDefault))
            {
                m.IsDefault = false;
                await _repository.UpdateAsync(m, ct);
            }
        }

        var method = new PaymentMethod
        {
            UserId = userId,
            Type = Enum.Parse<PaymentMethodType>(request.Type),
            Provider = request.Provider,
            Token = request.Token,
            LastFour = request.LastFour,
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            IsDefault = shouldBeDefault,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(method, ct);
        await _repository.SaveChangesAsync(ct);

        return _mapper.Map<PaymentMethodDto>(method);
    }

    public async Task SetDefaultAsync(Guid userId, Guid paymentMethodId, CancellationToken ct = default)
    {
        var methods = await _repository.GetByUserIdAsync(userId, ct);
        var target = methods.FirstOrDefault(m => m.Id == paymentMethodId && m.IsActive);

        if (target is null)
            throw new KeyNotFoundException("Payment method not found.");

        foreach (var m in methods.Where(m => m.IsDefault))
        {
            m.IsDefault = false;
            await _repository.UpdateAsync(m, ct);
        }

        target.IsDefault = true;
        await _repository.UpdateAsync(target, ct);
        await _repository.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(Guid userId, Guid paymentMethodId, CancellationToken ct = default)
    {
        var method = await _repository.GetByIdAsync(paymentMethodId, ct);

        if (method is null || method.UserId != userId)
            throw new KeyNotFoundException("Payment method not found.");

        method.IsActive = false;

        if (method.IsDefault)
        {
            method.IsDefault = false;
            var others = await _repository.GetByUserIdAsync(userId, ct);
            var nextDefault = others.FirstOrDefault(m => m.Id != paymentMethodId && m.IsActive);
            if (nextDefault is not null)
            {
                nextDefault.IsDefault = true;
                await _repository.UpdateAsync(nextDefault, ct);
            }
        }

        await _repository.UpdateAsync(method, ct);
        await _repository.SaveChangesAsync(ct);
    }
}
