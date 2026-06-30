# Real-Time Auction Platform — Architecture Document

## 1. Overview

The Real-Time Auction Platform is a concurrent live-bidding system built on **ASP.NET Core Web API** (backend) and **Angular** (frontend), using **SignalR** for real-time bid synchronization. The system follows **Clean Architecture** principles, separating business logic from infrastructure concerns to maximize testability, maintainability, and long-term scalability.

**Core capabilities:**
- Live, concurrent bidding with race-condition-safe bid acceptance
- Real-time notifications (outbid alerts, auction status changes)
- Auto-bidding (proxy bidding up to a user-defined max)
- Delivery and payment settlement after auction close

---

## 2. Architectural Style: Clean Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     Presentation                         │
│         (Web API Controllers, SignalR Hubs)               │
├─────────────────────────────────────────────────────────┤
│                      Application                          │
│      (Use Cases, Services, DTOs, Validators, MediatR)     │
├─────────────────────────────────────────────────────────┤
│                        Domain                              │
│        (Entities, Enums, Value Objects, Domain Logic)     │
├─────────────────────────────────────────────────────────┤
│                     Infrastructure                          │
│   (EF Core, SQL Server, Redis, External APIs, Email)      │
└─────────────────────────────────────────────────────────┘
```

**Dependency rule:** dependencies flow *inward only*. Domain has zero dependencies. Infrastructure depends on Application/Domain abstractions (interfaces), never the reverse.

| Layer | Responsibility | Depends On |
|---|---|---|
| Domain | Entities, enums, core business rules | Nothing |
| Application | Use cases, service interfaces, DTOs, validation | Domain |
| Infrastructure | EF Core DbContext, repositories, SignalR persistence, external integrations | Application (interfaces) |
| Presentation | Controllers, SignalR Hubs, request/response mapping | Application |

This is why your `Domain/Entities` (generated earlier) have zero EF Core Fluent API code — only the `[Timestamp]` attribute, which is a deliberate, minimal exception.

---

## 3. Layered Project Structure

```
AuctionPlatform/
├── AuctionPlatform.Domain/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── Auction.cs
│   │   ├── AuctionItem.cs
│   │   ├── AuctionItemImage.cs
│   │   ├── Bid.cs
│   │   ├── AutoBid.cs
│   │   ├── AuctionWinner.cs
│   │   ├── Notification.cs
│   │   ├── PaymentMethod.cs
│   │   └── AuctionCategory.cs
│   └── Enums/
│       └── Enums.cs
│
├── AuctionPlatform.Application/
│   ├── Interfaces/
│   │   ├── IAuctionRepository.cs
│   │   ├── IBidService.cs
│   │   ├── INotificationService.cs
│   │   └── IUnitOfWork.cs
│   ├── Services/
│   │   ├── BidService.cs
│   │   ├── AuctionService.cs
│   │   └── AutoBidService.cs
│   ├── DTOs/
│   │   ├── BidRequestDto.cs
│   │   ├── AuctionResponseDto.cs
│   │   └── ...
│   └── Validators/
│       └── BidRequestValidator.cs
│
├── AuctionPlatform.Infrastructure/
│   ├── Persistence/
│   │   ├── AuctionDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── BidConfiguration.cs
│   │   │   └── AuctionConfiguration.cs
│   │   └── Repositories/
│   │       ├── AuctionRepository.cs
│   │       └── BidRepository.cs
│   ├── Caching/
│   │   └── RedisCacheService.cs
│   └── Migrations/
│
├── AuctionPlatform.API/
│   ├── Controllers/
│   │   ├── AuctionsController.cs
│   │   ├── BidsController.cs
│   │   └── PaymentsController.cs
│   ├── Hubs/
│   │   └── AuctionHub.cs
│   └── Program.cs
│
└── auction-platform-client/    (Angular)
    ├── src/app/
    │   ├── features/
    │   │   ├── auction-list/
    │   │   ├── auction-detail/
    │   │   └── bid-panel/
    │   ├── services/
    │   │   ├── auction.service.ts
    │   │   └── signalr.service.ts
    │   └── shared/
```

---

## 4. Core Domain Flow: Placing a Bid

This is the most concurrency-sensitive path in the system. Every step matters for correctness under load.

```
Client (Angular)
   │
   │ POST /api/auctions/{id}/bids
   ▼
BidsController
   │
   │ → BidRequestValidator (FluentValidation)
   ▼
BidService.PlaceBidAsync()
   │
   ├─► 1. Load Auction (with RowVersion / concurrency token)
   ├─► 2. Validate: Status == Live, Amount >= CurrentHighestBid + BidIncrement
   ├─► 3. Begin DB transaction
   ├─► 4. Insert Bid row
   ├─► 5. Update Auction.CurrentHighestBid
   ├─► 6. SaveChangesAsync() ──► EF Core checks RowVersion
   │         │
   │         ├─ Success → commit transaction
   │         └─ Conflict → DbUpdateConcurrencyException
   │                          │
   │                          ▼
   │                  Retry / Reject bid, return 409
   │
   ├─► 7. Mark previous highest bid as "Outbid"
   ├─► 8. Trigger AutoBid evaluation (if applicable)
   └─► 9. Publish to SignalR Hub
              │
              ▼
   AuctionHub.BroadcastNewBid()
              │
              ├──► All clients in Auction group receive "BidPlaced" event
              ├──► Previous highest bidder receives "OutbidAlert" notification
              └──► Notification persisted to DB (for offline users)
```

**Why this design holds under concurrency:**
- The `RowVersion` token on `Bid`/`Auction` means two simultaneous bids can't silently overwrite each other — the second write fails fast and the service layer decides whether to retry against the new state or reject outright.
- The DB transaction wraps both the `Bid` insert and the `Auction.CurrentHighestBid` update — partial writes (a bid recorded but the auction's highest-bid pointer not updated) are impossible.
- SignalR broadcast happens **after** the transaction commits, not before — clients never see a bid that didn't actually persist.

---

## 5. SignalR Real-Time Architecture

```
                     ┌─────────────────────┐
                     │     AuctionHub       │
                     │  (SignalR Hub)        │
                     └──────────┬────────────┘
                                │
            ┌───────────────────┼───────────────────┐
            │                   │                     │
      Group: "auction-{id}"  Group: "user-{id}"   Group: "global"
            │                   │                     │
   ┌────────┴────────┐  ┌──────┴───────┐    ┌────────┴────────┐
   │ All viewers of    │  │ Personal      │    │ Site-wide        │
   │ this auction      │  │ notifications  │    │ announcements    │
   │ - BidPlaced        │  │ - OutbidAlert  │    │ - NewAuctionLive │
   │ - CountdownTick    │  │ - YouWon       │    │                   │
   │ - AuctionEnded     │  │ - PaymentDue   │    │                   │
   └────────────────────┘  └────────────────┘    └───────────────────┘
```

**Connection lifecycle:**
1. Client connects → `OnConnectedAsync()` → joins `user-{userId}` group automatically (from JWT claims)
2. Client navigates to an auction page → invokes `JoinAuctionGroup(auctionId)` → joins `auction-{id}` group
3. Client navigates away → `LeaveAuctionGroup(auctionId)`
4. Disconnect → `OnDisconnectedAsync()` → cleanup, no explicit group removal needed (SignalR handles it)

**Multi-server scaling consideration:** if you deploy more than one API instance, SignalR's in-memory backplane won't broadcast across servers. You need the **Redis backplane** (`AddStackExchangeRedis()`) so a bid placed on Server A reaches a client connected to Server B.

---

## 6. Auto-Bid Evaluation Flow

```
New Bid Placed (Amount = X)
        │
        ▼
Are there active AutoBids on this auction
with MaxAmount > X, excluding the current bidder?
        │
   ┌────┴────┐
   │   Yes    │
   ▼          │
Select AutoBid with highest MaxAmount
        │
        ▼
Place system-generated Bid at:
   min(MaxAmount, CurrentHighestBid + BidIncrement)
        │
        ▼
Loop back into "Placing a Bid" flow
(treated identically — no special-casing downstream)
```

**Design rationale:** auto-bids are *not* a separate code path through validation/persistence — they re-enter the same `BidService.PlaceBidAsync()` pipeline with `IsAutoBid = true`. This avoids duplicating concurrency-safety logic and keeps the rules in one place.

---

## 7. Auction Lifecycle State Machine

```
   Draft ──────► Scheduled ──────► Live ──────► Ended
     │                                │            │
     │                                │            ├──► ReserveNotMet
     │                                │            │     (final price < ReservePrice)
     │                                │            │
     │                                ▼            ▼
     └────────────────────────► Cancelled    AuctionWinner created
                                              (final price >= ReservePrice)
```

**Triggers:**
- `Draft → Scheduled`: seller publishes with a future `StartTime`
- `Scheduled → Live`: background job (hosted service / scheduled trigger) at `StartTime`
- `Live → Ended`: background job at `EndTime`, OR last bid forces extension (anti-snipe rule, if implemented)
- `Ended → AuctionWinner`: triggered synchronously when status flips to `Ended` — evaluates `CurrentHighestBid >= ReservePrice`

---

## 8. Post-Auction Settlement Flow

```
Auction.Status → Ended
        │
        ▼
Evaluate: CurrentHighestBid >= ReservePrice?
        │
   ┌────┴─────┐
   No          Yes
   │            │
   ▼            ▼
Status =    Create AuctionWinner
ReserveNotMet      │
   │               ├─► Link WinningBidId
   │               ├─► Set FinalPrice
   │               ├─► Status: PaymentStatus.Pending
   │               │
   │               ▼
   │         Notify winner (NotificationType.YouWon)
   │               │
   │               ▼
   │         Winner selects PaymentMethod
   │               │
   │               ▼
   │         Payment processed (external gateway)
   │               │
   │          ┌────┴────┐
   │        Success    Failure
   │          │           │
   │          ▼           ▼
   │    PaymentStatus  PaymentStatus
   │    = Paid          = Failed
   │          │           │
   │          ▼           ▼
   │    Calculate     Notify winner,
   │    ShippingCost  retry / re-list
   │    by DeliveryType
   │          │
   │          ▼
   │    DeliveryStatus
   │    = Pending → Shipped/PickedUp → Delivered
   ▼
(Auction closed, no winner record)
```

---

## 9. Cross-Cutting Concerns

| Concern | Implementation Approach |
|---|---|
| **Authentication** | JWT bearer tokens; SignalR Hub reads identity from `HttpContext.User` on connection |
| **Authorization** | Role-based (`Admin`, `Seller`, `Bidder`) via `[Authorize(Roles = "...")]` |
| **Concurrency control** | EF Core optimistic concurrency (`RowVersion`) on `Bid`/`Auction` |
| **Caching** | Redis — `CurrentHighestBid` cached to avoid DB round-trip on every bid validation |
| **Background jobs** | Hosted services (or Hangfire) for auction start/end transitions |
| **Real-time transport** | SignalR with Redis backplane for horizontal scaling |
| **Validation** | FluentValidation at the Application layer, before hitting the service |
| **Logging/Observability** | Structured logging (Serilog) + correlation IDs across HTTP → SignalR → DB |

---

## 10. Key Architectural Decisions & Tradeoffs

| Decision | Alternative Considered | Why This Choice |
|---|---|---|
| Optimistic concurrency (RowVersion) | Pessimistic locking (`SELECT ... FOR UPDATE`) | Optimistic scales better under high read/low-conflict-rate bidding; pessimistic locks would serialize all bids on hot auctions, killing throughput |
| Auto-bid re-enters same pipeline | Separate auto-bid execution path | Avoids duplicated validation/concurrency logic; one source of truth for "what makes a valid bid" |
| SignalR groups per auction | Broadcast to all connected clients | Reduces unnecessary network chatter — a user watching Auction A doesn't need updates from Auction B |
| Delivery type on `Auction`, not `AuctionItem` | Per-item delivery type | All items in one auction ship under the same terms; simpler settlement logic. Revisit if mixed digital/physical lots become a requirement |
| Redis backplane for SignalR | Sticky sessions / single-server SignalR | Sticky sessions don't survive instance failure and block horizontal autoscaling — Redis backplane is the correct production pattern |

---

## 11. Summary

The architecture cleanly separates **what the business rules are** (Domain) from **how they're enforced and persisted** (Application/Infrastructure) from **how users interact with the system** (Presentation/Angular). The bidding hot path is designed explicitly around optimistic concurrency to handle simultaneous bids correctly without sacrificing throughput, and SignalR's group-based broadcast model keeps real-time updates scoped and efficient. The auction lifecycle and settlement flow are modeled as explicit state machines, making the system's behavior predictable and testable at each transition.

**Next architectural decisions to formalize:** anti-sniping rules (auto-extend auction on last-second bids), idempotency keys for bid submission (handle client retries safely), and dead-letter handling for failed SignalR broadcasts.
