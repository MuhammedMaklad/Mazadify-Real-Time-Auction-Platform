using AuctionPlatform.Application.Common.Interfaces;
using AuctionPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionPlatform.Infrastructure.Persistence.Repositories;

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly AppDbContext _dbContext;

    public PaymentMethodRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PaymentMethod>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _dbContext.PaymentMethods
            .Where(p => p.UserId == userId && p.IsActive)
            .OrderByDescending(p => p.IsDefault)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<PaymentMethod?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbContext.PaymentMethods.FindAsync([id], ct);
    }

    public async Task AddAsync(PaymentMethod paymentMethod, CancellationToken ct = default)
    {
        await _dbContext.PaymentMethods.AddAsync(paymentMethod, ct);
    }

    public Task UpdateAsync(PaymentMethod paymentMethod, CancellationToken ct = default)
    {
        _dbContext.PaymentMethods.Update(paymentMethod);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _dbContext.SaveChangesAsync(ct);
    }
}
