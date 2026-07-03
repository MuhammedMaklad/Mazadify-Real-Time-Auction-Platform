# Mazadify — Real-Time Auction Platform Architecture

## 1. Overview

Mazadify is a **concurrent live-bidding auction platform** built with Clean Architecture on .NET 10. It supports real-time bidding via SignalR (planned), auto-bidding/proxy bidding, auction lifecycle management, and post-auction settlement.

## 2. Technology Stack

| Layer | Technology | Version |
|---|---|---|
| Backend runtime | ASP.NET Core | 10.0 |
| Language | C# | 13 |
| ORM | Entity Framework Core | 10.0 |
| Database | SQL Server | — |
| Validation | FluentValidation | 12.1 |
| Mapping | AutoMapper | 13.0 |
| Real-time | SignalR (planned) | — |
| Auth | JWT Bearer (planned) | — |
| Frontend | Angular | 21.2 |

## 3. Solution Structure — Clean Architecture

`
backend/
├── AuctionPlatform.Domain/              # Layer 0: Enterprise business rules
│   ├── Entities/                         #   Domain models (Auction, User, Bid, etc.)
│   └── ValueTypes/                       #   Enums (AuctionStatus, BidStatus, etc.)
│
├── AuctionPlatform.Application/         # Layer 1: Application business rules
│   ├── Auctions/
│   │   ├── DTOs/                         #   Data transfer objects
│   │   ├── Interfaces/                   #   Service contracts
│   │   ├── Services/                     #   Use case implementations
│   │   └── Validators/                   #   FluentValidation rules
│   ├── Categories/
│   │   ├── DTOs/
│   │   ├── Interfaces/
│   │   └── Services/
│   ├── Common/
│   │   ├── Interfaces/                   #   Repository contracts
│   │   └── Models/                       #   PagedResult<T>
│   └── Mappings/                         #   AutoMapper profiles
│
├── AuctionPlatform.Infrastructure/      # Layer 2: External concerns
│   └── Persistence/
│       ├── Configurations/               #   EF Core Fluent API configs
│       ├── Repositories/                 #   Repository implementations
│       ├── AppDbContext.cs                #   EF Core DbContext
│       ├── DbSeeder.cs                   #   Seed data
│       └── DependencyInjection.cs        #   Service registration
│
└── AuctionPlatform.WebApi/              # Layer 3: Presentation
    ├── Controllers/                       #   API endpoints
    └── Program.cs                         #   Host configuration
`

### Dependency Graph

`
WebApi → Application → Domain
WebApi → Infrastructure → Application → Domain
`

## 4. Domain Model — Auction Feature

### Entity Relationship Diagram

\\\mermaid
erDiagram
    User ||--o{ Auction : "sells"
    Auction ||--|| AuctionCategory : "classified as"
    Auction ||--o{ AuctionItem : "contains"
    AuctionItem ||--o{ AuctionItemImage : "has images"
    Auction ||--o| AuctionWinner : "results in"
    AuctionCategory ||--o{ AuctionCategory : "self-referencing hierarchy"
\\\

### Core Entities

| Entity | Key Fields | Notes |
|---|---|---|
| **User** | Id, Username, Email, PasswordHash | Seller & bidder identity |
| **Auction** | Id, SellerId, CategoryId, Title, StartingPrice, ReservePrice, CurrentHighestBid, BidIncrement, StartTime, EndTime, Status, DeliveryType, IsDeleted | Central entity with lifecycle |
| **AuctionCategory** | Id, Name, Slug, ParentCategoryId | Self-referencing tree hierarchy |
| **AuctionItem** | Id, AuctionId, Name, Description, Condition | Line items within an auction |
| **AuctionItemImage** | Id, AuctionItemId, ImageUrl, IsPrimary, DisplayOrder | Product images |
| **AuctionWinner** | Id, AuctionId, WinnerId, WinningBidId, FinalPrice, PaymentStatus, DeliveryStatus | Post-auction settlement |

### Auction Lifecycle

`
Draft → Scheduled → Live → Ended → ReserveNotMet
                                → Winner awarded (via AuctionWinner)
         Cancelled (any state before Ended)
`

## 5. Database Design (EF Core Configuration)

### Table Mappings

| Entity | Table | PK Strategy |
|---|---|---|
| User | Users | NEWSEQUENTIALID() |
| Auction | Auctions | NEWSEQUENTIALID() |
| AuctionCategory | AuctionCategories | NEWSEQUENTIALID() |
| AuctionItem | AuctionItems | NEWSEQUENTIALID() |
| AuctionItemImage | AuctionItemImages | NEWSEQUENTIALID() |
| AuctionWinner | AuctionWinners | NEWSEQUENTIALID() |

### Enum Storage

All enums stored as **strings** (\archar(20)\) using \HasConversion<string>()\:

- \AuctionStatus\ → Draft, Scheduled, Live, Ended, Cancelled, ReserveNotMet
- \DeliveryType\ → Pickup, Shipping, Digital
- \PaymentStatus\ → Pending, Paid, Failed, Refunded
- \DeliveryStatus\ → Pending, Shipped, Delivered, PickedUp, Cancelled

### Soft Delete

\Auction\ and \AuctionItem\ use soft delete:

\\\csharp
public bool IsDeleted { get; set; }
public DateTime? DeletedAt { get; set; }
\\\

Applied via global query filter:

\\\csharp
builder.HasQueryFilter(e => !e.IsDeleted);
\\\

### Decimal Precision

All monetary values use \decimal(18,2)\:

- \StartingPrice\, \ReservePrice\, \CurrentHighestBid\, \BidIncrement\
- \FinalPrice\, \ShippingCost\

### Indexes

| Table | Index | Type |
|---|---|---|
| Auctions | SellerId | Non-clustered |
| Auctions | CategoryId | Non-clustered |
| Auctions | Status | Non-clustered |
| Auctions | StartTime, EndTime | Composite |
| AuctionCategories | Slug | Unique |
| AuctionCategories | ParentCategoryId | Non-clustered |
| AuctionItems | AuctionId | Non-clustered |
| AuctionItemImages | AuctionItemId | Non-clustered |
| AuctionWinners | AuctionId | Unique |
| AuctionWinners | WinnerId | Non-clustered |
| Users | Email | Unique |
| Users | Username | Unique |

### Cascade Behavior

| Relationship | Delete Behavior |
|---|---|
| Auction → Items | Cascade |
| AuctionItem → Images | Cascade |
| Auction → Seller | Restrict |
| Auction → Category | Restrict |
| Auction → Winner | Restrict |
| Category → Parent Category | Restrict |
| AuctionWinner → Winner | Restrict |
| AuctionWinner → PaymentMethod | Set Null |

## 6. API Endpoints

| Method | Path | Controller Action | Status | Description |
|---|---|---|---|---|
| \GET\ | \/api/auctions\ | \GetList\ | \200\ | Paginated list with search/filter/sort |
| \GET\ | \/api/auctions/{id}\ | \GetById\ | \200\ / \404\ | Full auction detail with items |
| \POST\ | \/api/auctions\ | \Create\ | \201\ / \400\ / \404\ | Create new auction (Draft status) |
| \PUT\ | \/api/auctions/{id}\ | \Update\ | \200\ / \400\ / \404\ | Partial update (Draft/Scheduled only) |
| \DELETE\ | \/api/auctions/{id}\ | \Delete\ | \204\ / \400\ / \404\ | Soft delete (Draft only) |
| \GET\ | \/api/categories\ | \GetCategoryTree\ | \200\ | Hierarchical category tree |
| \GET\ | \/api/categories/{slug}\ | \GetBySlug\ | \200\ / \404\ | Category lookup by slug |

### Error Handling

| Exception | HTTP Status |
|---|---|
| \KeyNotFoundException\ | \404 Not Found\ |
| \InvalidOperationException\ | \400 Bad Request\ |
| FluentValidation \ValidationException\ | \400 Bad Request\ |

## 7. FluentValidation Rules

### CreateAuctionRequest
| Field | Rules |
|---|---|
| CategoryId | NotEmpty |
| Title | NotEmpty, Max 200 |
| Description | NotEmpty, Max 5000 |
| StartingPrice | GreaterThan 0 |
| ReservePrice | GreaterThanOrEqualTo 0 |
| BidIncrement | GreaterThan 0 |
| StartTime | GreaterThan UtcNow, LessThan EndTime |
| EndTime | GreaterThan StartTime |
| DeliveryType | NotEmpty, Max 20 |
| Items | NotEmpty |
| Items[].Name | NotEmpty, Max 200 |
| Items[].Description | NotEmpty, Max 2000 |
| Items[].Condition | NotEmpty, Max 30 |

### UpdateAuctionRequest
- All fields optional
- At least one field required
- Same per-field rules as create when provided

### AuctionListFilter
| Field | Rules |
|---|---|
| Page | GreaterThan 0 |
| PageSize | 1–100 |
| SortBy | Must be: \startTime\, \ndTime\, \currentHighestBid\, \	itle\ |

## 8. AutoMapper Mappings

| Source → Destination | Notes |
|---|---|
| \Auction\ → \AuctionDto\ | Includes Category, Seller, Items with Images; Status & DeliveryType converted to string |
| \Auction\ → \AuctionSummaryDto\ | Flattened: CategoryName, SellerUsername, ItemCount, PrimaryImageUrl (first primary image) |
| \AuctionItem\ → \AuctionItemDto\ | Includes Images collection |
| \AuctionItemImage\ → \AuctionItemImageDto\ | Direct mapping |
| \User\ → \UserBriefDto\ | Only Id + Username |
| \AuctionCategory\ → \AuctionCategoryDto\ | Direct mapping |

## 9. Seed Data

### AuctionCategories (16 — via \HasData()\ in migration)

| Parent | Children |
|---|---|
| Electronics | Computers, Smartphones & Accessories, Audio & Headphones, Gaming |
| Collectibles & Art | Art, Collectibles |
| Vehicles | Cars, Motorcycles, Bicycles |
| Home & Garden | Furniture, Appliances, Tools & Equipment |

### Users (3 — via \DbSeeder\)

| Username | Email | Role |
|---|---|---|
| admin | admin@mazadify.com | Admin |
| seller1 | seller1@mazadify.com | Seller |
| seller2 | seller2@mazadify.com | Seller |

All passwords BCrypt-hashed.

### Auctions (8)

| Title | Status | Starting Price |
|---|---|---|
| Vintage Camera Collection | Draft |  |
| Gaming Laptop Pro X | Scheduled | ,200 |
| Wireless Noise-Canceling Headphones | Live |  |
| Antique Wooden Desk (with chair) | Live |  |
| Classic Mustang 1967 | Ended | ,000 |
| Latest Smartphone Bundle | ReserveNotMet |  |
| PS5 Pro + 5 Games Bundle | Live |  |
| Professional Tool Set | Scheduled |  |

### Items & Images
- 12 auction items across 8 auctions
- 2 placeholder images per item (primary + secondary)

## 10. Key Design Decisions

### GUID vs INT for Primary Keys
- **Decision**: GUIDs with \NEWSEQUENTIALID()\
- **Why**: Prevents URL enumeration attacks on auction IDs; enables client-side ID generation; supports future distributed scaling
- **Mitigation**: \NEWSEQUENTIALID()\ in SQL Server prevents index fragmentation

### Enum Storage as Strings
- **Decision**: \HasConversion<string>()\ with \archar(20)\
- **Why**: Readable in database queries; flexible for adding new enum values; avoids magic numbers

### Soft Delete
- **Decision**: \IsDeleted\ + \DeletedAt\ on Auction and AuctionItem
- **Why**: Data retention for audit/compliance; ability to restore; prevents orphaned references
- **Implementation**: Global query filter \HasQueryFilter(e => !e.IsDeleted)\

### Repository + Service Pattern
- **Decision**: Separate repository interfaces (data access) and service interfaces (business logic)
- **Why**: Single responsibility; testability (repositories can be mocked); infrastructure-agnostic application layer

### Manual Validation in Controllers
- **Decision**: Inject \IValidator<T>\ and call \ValidateAsync()\ in controller actions
- **Why**: Explicit control over validation flow; no hidden auto-validation behavior; clear where validation happens

### Fixed Seed GUIDs
- **Decision**: Use deterministic GUIDs for seed data (e.g., \A1000000-...\)
- **Why**: Enables consistent references across seed data (categories → auctions); usable in HTTP test file for dev/testing

## 11. Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server (local or Docker)
- Node.js 20+ (for frontend)

### Running the Backend
\\\ash
cd backend
# Update connection string in appsettings.Development.json
dotnet run --project AuctionPlatform.WebApi
\\\

The API starts at \http://localhost:5213\ with OpenAPI at \/openapi/v1.json\.

### Database
Database is created automatically on first run via \EnsureCreatedAsync()\. Seed data is applied idempotently (skips if users exist).
