# Bidding Engine Integration Checklist

## Current Status: ✅ Core Implementation Complete

The Bidding Engine (Member 3) is **complete** but needs integration with other modules when they finish.

---

## 🔴 CRITICAL ITEMS (Must Do Before Production)

### 1. Authentication & JWT Integration
**Status:** ❌ NOT DONE (TODO in code)
**Owner:** Member 6 (User Management & Authentication)
**What's needed:**
- Extract `bidderId` from JWT claims instead of hardcoded value
- Replace in `BidsController.PlaceBid()`:

```csharp
// CURRENT (Hardcoded):
var bidderId = Guid.Parse("B1000000-0000-0000-0000-000000000002");

// NEEDED:
var bidderId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
	?? throw new UnauthorizedAccessException("User ID not found in token"));
```

**File to modify:** `backend/AuctionPlatform.WebApi/Controllers/BidsController.cs` (line ~52)

**Dependency:** Member 6 must provide JWT token structure

---

### 2. Real-Time Notifications (SignalR)
**Status:** ⚠️ SKELETON ONLY
**Owner:** Member 4 (Real-Time Notifications)
**What's needed:**
- Replace `NoOpNotificationService` with real SignalR implementation
- Implement `INotificationService`:

```csharp
public class AuctionHubNotificationService : INotificationService
{
	private readonly IHubContext<AuctionHub> _hubContext;

	public async Task NotifyBidPlacedAsync(Guid auctionId, Guid bidderId, decimal amount, CancellationToken ct)
	{
		await _hubContext.Clients
			.Group($"auction-{auctionId}")
			.SendAsync("BidPlaced", new { auctionId, bidderId, amount }, cancellationToken: ct);
	}

	public async Task NotifyOutbidAsync(Guid auctionId, Guid previousBidderId, decimal currentHighestBid, CancellationToken ct)
	{
		await _hubContext.Clients
			.User(previousBidderId.ToString())
			.SendAsync("OutbidAlert", new { auctionId, currentHighestBid }, cancellationToken: ct);
	}
}
```

**Files to create:**
- `backend/AuctionPlatform.Infrastructure/Notifications/AuctionHubNotificationService.cs`
- `backend/AuctionPlatform.WebApi/Hubs/AuctionHub.cs`

**Register in DI:** Replace in `Infrastructure/Persistence/DependencyInjection.cs`
```csharp
services.AddScoped<INotificationService, AuctionHubNotificationService>();
```

**Dependency:** Member 4 must design AuctionHub structure

---

### 3. User Identity Population (Foreign Keys)
**Status:** ⚠️ PARTIAL
**Owner:** Member 6 (User Management)
**What's needed:**
- Ensure `Bid.Bidder` navigation property is populated when loading bids
- Update `BidRepository.GetHistoryAsync()` and `GetHighestBidAsync()` to include User:

```csharp
public async Task<Bid?> GetHighestBidAsync(Guid auctionId, CancellationToken ct = default)
{
	return await _dbContext.Set<Bid>()
		.Include(b => b.Bidder)  // ADD THIS
		.Where(b => b.AuctionId == auctionId)
		.OrderByDescending(b => b.Amount)
		.ThenBy(b => b.PlacedAt)
		.FirstOrDefaultAsync(ct);
}
```

**Files to modify:**
- `backend/AuctionPlatform.Infrastructure/Persistence/Repositories/BidRepository.cs`

---

## 🟡 IMPORTANT ITEMS (Should Do Soon)

### 4. Authorization & Role Checks
**Status:** ❌ NOT DONE
**Owner:** Member 6 (User Management)
**What's needed:**
- Add `[Authorize]` attributes to controller
- Verify user can only bid if they're not the auction seller
- Currently only checked at service level, should also check at controller

**File to modify:** `backend/AuctionPlatform.WebApi/Controllers/BidsController.cs`

```csharp
[Authorize]
[HttpPost]
public async Task<IActionResult> PlaceBid(...)
{
	// Verify authenticated user
	if (!User.Identity?.IsAuthenticated ?? false)
		return Unauthorized();

	var bidderId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
		?? throw new UnauthorizedAccessException());

	// Rest of method...
}
```

---

### 5. Idempotency Implementation
**Status:** ❌ DEFERRED (Phase 5)
**Owner:** Backend team (whoever is handling Phase 5)
**What's needed:**
- Implement `IIdempotencyCache` interface
- Prevent duplicate bids if client retries same request
- Short-lived cache (5-10 minutes)

**File to create:** `backend/AuctionPlatform.Application/Common/Interfaces/IIdempotencyCache.cs`

```csharp
public interface IIdempotencyCache
{
	Task<bool> IsIdempotencyProcessedAsync(string idempotencyKey, CancellationToken ct = default);
	Task MarkAsProcessedAsync(string idempotencyKey, TimeSpan ttl, CancellationToken ct = default);
}
```

**File to create:** `backend/AuctionPlatform.Infrastructure/Caching/DistributedIdempotencyCache.cs`

**Update BidService to use it:**
```csharp
if (!string.IsNullOrEmpty(request.IdempotencyKey))
{
	if (await _idempotencyCache.IsIdempotencyProcessedAsync(request.IdempotencyKey, ct))
		throw new InvalidOperationException("This bid has already been processed.");
}

// ... place bid ...

if (!string.IsNullOrEmpty(request.IdempotencyKey))
{
	await _idempotencyCache.MarkAsProcessedAsync(request.IdempotencyKey, TimeSpan.FromMinutes(10), ct);
}
```

---

### 6. Rate Limiting
**Status:** ❌ NOT DONE
**Owner:** Backend team
**What's needed:**
- Prevent bid spam (e.g., max 10 bids per user per auction per minute)
- Per-IP rate limiting

**Options:**
- Use AspNetCore.RateLimit NuGet package
- Or implement custom middleware

**Add to Program.cs:**
```csharp
services.AddRateLimiter(options =>
{
	options.AddFixedWindowLimiter(
		policyName: "bid-limiter",
		configure: options =>
		{
			options.PermitLimit = 10;
			options.Window = TimeSpan.FromMinutes(1);
			options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
			options.QueueLimit = 5;
		});
});
```

---

### 7. Logging & Audit Trail
**Status:** ⚠️ BASIC ONLY
**Owner:** Backend team
**What's needed:**
- Log all bid placements (success & failure)
- Track IP address (already captured in Bid entity)
- Structured logging with correlation IDs

**Current:** BidService logs via dependency injection, but no custom audit logic

**File to create:** `backend/AuctionPlatform.Application/Services/BidAuditService.cs`

```csharp
public class BidAuditService
{
	private readonly ILogger<BidAuditService> _logger;

	public void LogBidPlaced(Guid auctionId, Guid bidderId, decimal amount, string? ipAddress)
	{
		_logger.LogInformation(
			"Bid placed: AuctionId={AuctionId}, BidderId={BidderId}, Amount={Amount}, IP={IpAddress}",
			auctionId, bidderId, amount, ipAddress);
	}
}
```

---

### 8. Anti-Sniping (Optional but Recommended)
**Status:** ❌ NOT DONE
**Owner:** Backend team (Phase 6+)
**What's needed:**
- Auto-extend auction if bid placed within last N seconds (e.g., 30 seconds)
- Update `Auction.EndTime` before SaveChanges

**Logic:**
```csharp
const int ANTI_SNIPE_SECONDS = 30;
var secondsUntilEnd = (auction.EndTime - DateTime.UtcNow).TotalSeconds;

if (secondsUntilEnd < ANTI_SNIPE_SECONDS)
{
	auction.EndTime = DateTime.UtcNow.AddSeconds(ANTI_SNIPE_SECONDS);
	// Notify auction viewers that time extended
}
```

---

## 🟢 INTEGRATION CHECKLIST (When Members Finish)

### When Member 1 Finishes (Auction Lifecycle)
- [ ] Verify `Auction.Status` transitions work with bidding
- [ ] Test: Can't bid on Draft/Scheduled/Cancelled auctions
- [ ] Test: Auction transitions to `Ended` correctly
- [ ] Verify: When auction ends, determine winner from `Bids` table

**Files affected:**
- `backend/AuctionPlatform.Domain/ValueTypes/Enums.cs` (AuctionStatus enum)
- `backend/AuctionPlatform.Domain/Entities/Auction.cs` (Status property)

---

### When Member 2 Finishes (Payment & Settlement)
- [ ] After auction ends, identify highest bidder
- [ ] Create `AuctionWinner` record
- [ ] Link `AuctionWinner.WinningBidId` to highest `Bid`
- [ ] Trigger payment process

**Integration point:**
```csharp
// In AuctionService or new AuctionSettlementService:
if (auction.Status == AuctionStatus.Ended)
{
	var highestBid = await _bidRepository.GetHighestBidAsync(auction.Id, ct);
	if (highestBid?.Amount >= auction.ReservePrice)
	{
		var winner = new AuctionWinner
		{
			AuctionId = auction.Id,
			WinnerId = highestBid.BidderId,
			WinningBidId = highestBid.Id,
			FinalPrice = highestBid.Amount,
			// ... payment settlement fields
		};
		await _auctionWinnerRepository.AddAsync(winner, ct);
	}
}
```

**Files to create/modify:**
- `backend/AuctionPlatform.Application/Common/Interfaces/IAuctionWinnerRepository.cs`
- `backend/AuctionPlatform.Infrastructure/Persistence/Repositories/AuctionWinnerRepository.cs`

---

### When Member 4 Finishes (Real-Time Notifications)
- [ ] Replace `NoOpNotificationService` (see Critical Item #2 above)
- [ ] Test: BidPlaced events broadcast to auction viewers
- [ ] Test: OutbidAlert sent to previous bidder
- [ ] Verify: Notifications only sent after transaction commits

---

### When Member 5 Finishes (Search & Discovery)
- [ ] Add bid count to auction search results
- [ ] Add current highest bid to search results
- [ ] Filter auctions by bid count
- [ ] Sort by recent bids

**Files to create/modify:**
- Update `AuctionSummaryDto` to include `CurrentHighestBid`, `BidCount`
- Update search query in `AuctionRepository.GetFilteredAsync()`

---

### When Member 6 Finishes (User Management & Authentication)
- [ ] Implement JWT extraction (Critical Item #1 above)
- [ ] Add `[Authorize]` attribute to controllers
- [ ] Verify bidder identity
- [ ] Prevent seller from bidding
- [ ] Track user bid history

---

## 📋 DATABASE SCHEMA ADDITIONS NEEDED

### When Member 1 Finishes
- Verify `Auctions.Status` enum values match domain
- Verify `Auctions.StartTime`, `Auctions.EndTime` are correct types

### When Member 2 Finishes
- Ensure `AuctionWinner.WinningBidId` foreign key exists
- Ensure `Bids.AuctionWinner` navigation property works
- Create index on `AuctionWinners.AuctionId` for fast lookup

### Already Done (Member 3)
- ✅ `Bids` table created
- ✅ RowVersion column for optimistic concurrency
- ✅ Indexes on `AuctionId`, `BidderId`, `PlacedAt`
- ✅ Composite index on `(AuctionId, Amount DESC)`

---

## 🧪 TESTING CHECKLIST

### Unit Tests (Per Module)
- [ ] BidService validation logic
- [ ] BidRepository pagination
- [ ] Concurrency handling (DbUpdateConcurrencyException)

### Integration Tests
- [ ] Valid bid placement → database updated
- [ ] Outbid marking → previous bid status changed
- [ ] Auction status validation
- [ ] Seller can't bid
- [ ] Concurrent bids → 409 Conflict

### End-to-End Tests (With All Members)
- [ ] User registers (Member 6)
- [ ] Seller creates auction (Member 1)
- [ ] Bidder places bid (Member 3) ← We're here
- [ ] SignalR notifies viewers (Member 4)
- [ ] Auction ends (Member 1)
- [ ] Winner determined (Member 3)
- [ ] Payment processed (Member 2)
- [ ] Winner appears in search results (Member 5)

---

## 📊 METRICS TO TRACK

Once in production:
- Bids per second
- Concurrent bidders per auction
- 409 Conflict rate (should be < 1% under normal load)
- Average bid response time (should be < 100ms)
- Database connection pool health
- Notification delivery rate

---

## 🚀 DEPLOYMENT CHECKLIST

Before deploying to production:
- [ ] All authentication implemented
- [ ] Real-time notifications working
- [ ] Rate limiting configured
- [ ] Idempotency cache operational
- [ ] Database backups configured
- [ ] Monitoring/alerting set up
- [ ] Load testing passed (100+ concurrent bids)
- [ ] Security review completed

---

## 📞 COMMUNICATION WITH TEAM

### For Member 1 (Auction Lifecycle)
- "How do you handle auction status transitions?"
- "Can we add hooks for when auction ends?"
- "Do we need to trigger winner determination?"

### For Member 2 (Payment & Settlement)
- "When auction ends, we need to determine the winner from Bids table"
- "Can you create AuctionWinner record with WinningBidId?"
- "What payment methods do you support?"

### For Member 4 (Real-Time)
- "We have INotificationService interface ready"
- "Notifications should only send AFTER DB commit"
- "Need BidPlaced and OutbidAlert events"

### For Member 5 (Search & Discovery)
- "Need CurrentHighestBid in search results"
- "Need BidCount for each auction"
- "Can we sort by recent bids?"

### For Member 6 (User Management & Auth)
- "Need JWT token structure with user ID claim"
- "BidsController needs bidderId from User.FindFirst()"
- "How do we prevent seller from bidding?"

---

## 📝 REMAINING TODOs IN CODE

Search for "TODO" in codebase:

```csharp
// backend/AuctionPlatform.WebApi/Controllers/BidsController.cs (line 52)
// TODO: Extract bidderId from authenticated user (JWT claims)
```

---

## Summary: What's Missing

| Item | Priority | Owner | Status |
|---|---|---|---|
| JWT Authentication | 🔴 CRITICAL | Member 6 | ❌ |
| SignalR Integration | 🔴 CRITICAL | Member 4 | ❌ |
| User Navigation Loading | 🟡 IMPORTANT | Member 6 | ⚠️ |
| Authorization | 🟡 IMPORTANT | Member 6 | ❌ |
| Idempotency Cache | 🟡 IMPORTANT | Backend | ❌ |
| Rate Limiting | 🟡 IMPORTANT | Backend | ❌ |
| Audit Logging | 🟡 IMPORTANT | Backend | ⚠️ |
| Anti-Sniping | 🟢 NICE-TO-HAVE | Backend | ❌ |
| Auction Settlement | 🔴 CRITICAL | Member 2 | ❌ |
| Search Integration | 🟢 NICE-TO-HAVE | Member 5 | ❌ |

---

**Next Steps:**
1. Coordinate with Member 6 on JWT structure
2. Coordinate with Member 4 on SignalR hub design
3. Schedule integration testing when all modules ready
4. Plan deployment and monitoring
