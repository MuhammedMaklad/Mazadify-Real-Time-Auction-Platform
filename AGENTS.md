# Mazadify — Agent Instructions

## Project Overview
Concurrent live-bidding auction platform (ASP.NET Core 10 + Angular 21).

## Build Commands
- Build all: dotnet build AuctionPlatform.slnx (from ackend/)
- Run API: dotnet run --project AuctionPlatform.WebApi (from ackend/)
- Frontend dev: 
pm start (from rontend/)
- Frontend test: 
pm test (from rontend/)

## Code Style Conventions
- **Language**: C# 13, file-scoped namespaces, implicit usings enabled
- **Nullable**: enabled (<Nullable>enable</Nullable>)
- **Architecture**: Clean Architecture — Domain → Application → Infrastructure → WebApi
- **Naming**: PascalCase for classes/methods, Async suffix for async methods, camelCase for locals/params
- **DTOs**: Classes with { get; set; } + default = []; / = string.Empty;
- **Validators**: FluentValidation — one validator class per request type
- **Mappings**: AutoMapper profiles in Mappings/ folder
- **Enums**: Stored as strings in DB via HasConversion<string>()
- **Soft delete**: IsDeleted + DeletedAt + HasQueryFilter(e => !e.IsDeleted)
- **GUID PKs**: HasDefaultValueSql("NEWSEQUENTIALID()") in EF configs
- **Decimal**: HasColumnType("decimal(18,2)") for all monetary properties

## Project Structure
`
backend/
├── AuctionPlatform.Domain/              # Entities, Enums (zero dependencies)
├── AuctionPlatform.Application/         # DTOs, Validators, Services, Interfaces
├── AuctionPlatform.Infrastructure/      # EF Core, Repositories, Configurations, Seeder
└── AuctionPlatform.WebApi/              # Controllers, Program.cs, Startup
frontend/
└── src/
    └── app/                             # Angular components, services, routes
`

## Key Rules
- Do NOT modify Domain entities without verifying Application DTOs and Infrastructure configs match
- Always add FluentValidation rules for new request DTOs
- Register new services/repos in DependencyInjection.cs (Application or Infrastructure)
- Prefer existing patterns over introducing new dependencies
- Controllers inject validators explicitly (no auto-validation magic)
- Soft delete: only Auction and AuctionItem have IsDeleted; other entities use hard delete
- Enums use string conversion for DB storage, not integers
