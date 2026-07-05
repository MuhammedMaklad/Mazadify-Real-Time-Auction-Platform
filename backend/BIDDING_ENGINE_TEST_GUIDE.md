# Bidding Engine Testing Guide

## Overview
This document outlines manual and automated test scenarios for the concurrency-safe bidding engine.

---

## Part 1: Manual API Testing (Postman / Thunder Client)

### Prerequisites
1. Auction must exist and be in `Live` status
2. Auction must have a future `EndTime`
3. Multiple test user accounts (seller, bidder1, bidder2)

### Test Data Setup

**Sample Auction ID:** `550e8400-e29b-41d4-a716-446655440000`

**Sample User IDs:**
- Seller: `b1000000-0000-0000-0000-000000000001`
- Bidder1: `b1000000-0000-0000-0000-000000000002`
- Bidder2: `b1000000-0000-0000-0000-000000000003`

**Sample Auction Properties:**
- Status: Live
- StartingPrice: 100.00
- CurrentHighestBid: 0 (initially)
- BidIncrement: 10.00
- StartTime: (past)
- EndTime: (future)

---

### Test Scenario 1: Valid First Bid

**Endpoint:** `POST /api/auctions/{auctionId}/bids`

**Request Body:**
```json
{
  "amount": 100.00,
  "idempotencyKey": null
}
```

**Expected Response:** 201 Created
```json
{
  "id": "...",
  "auctionId": "550e8400-e29b-41d4-a716-446655440000",
  "bidderId": "b1000000-0000-0000-0000-000000000002",
  "amount": 100.00,
  "status": "Accepted",
  "placedAt": "2025-01-15T10:30:00Z",
  "isAutoBid": false,
  "bidder": {
	"id": "b1000000-0000-0000-0000-000000000002",
	"username": "bidder1"
  }
}
```

**Verification:**
- Check database: Bids table has new row
- Check database: Auction.CurrentHighestBid = 100.00
- Verify Auction.Bids navigation includes new bid

---

### Test Scenario 2: Valid Second Bid (Outbids First)

**Request:**
```json
{
  "amount": 115.00,
  "idempotencyKey": null
}
```

**Expected Response:** 201 Created

**Verification:**
- Auction.CurrentHighestBid = 115.00
- Previous bid status changed to `Outbid`
- New bid status = `Accepted`

---

### Test Scenario 3: Bid Below Minimum

**Request:**
```json
{
  "amount": 110.00
}
```

**Expected Response:** 400 Bad Request
```json
{
  "message": "Bid amount must be at least 125.00. Current highest bid is 115.00."
}
```

---

### Test Scenario 4: Seller Bids on Own Auction

**Using Seller ID as Bidder:** `b1000000-0000-0000-0000-000000000001`

**Request:**
```json
{
  "amount": 150.00
}
```

**Expected Response:** 400 Bad Request
```json
{
  "message": "Seller cannot place bids on their own auction."
}
```

---

### Test Scenario 5: Bid on Closed Auction

Change Auction.Status to `Ended` in database, then place bid.

**Expected Response:** 400 Bad Request
```json
{
  "message": "Cannot place bid on auction with status 'Ended'. Auction must be Live."
}
```

---

### Test Scenario 6: Get Bid History

**Endpoint:** `GET /api/auctions/{auctionId}/bids?page=1&pageSize=10`

**Expected Response:** 200 OK
```json
{
  "items": [
	{
	  "id": "...",
	  "auctionId": "550e8400-e29b-41d4-a716-446655440000",
	  "bidderId": "b1000000-0000-0000-0000-000000000003",
	  "amount": 115.00,
	  "status": "Accepted",
	  "placedAt": "2025-01-15T10:31:00Z",
	  "isAutoBid": false,
	  "bidder": { ... }
	},
	{
	  "id": "...",
	  "auctionId": "550e8400-e29b-41d4-a716-446655440000",
	  "bidderId": "b1000000-0000-0000-0000-000000000002",
	  "amount": 100.00,
	  "status": "Outbid",
	  "placedAt": "2025-01-15T10:30:00Z",
	  "isAutoBid": false,
	  "bidder": { ... }
	}
  ],
  "totalCount": 2,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

**Verification:**
- Bids ordered by PlacedAt descending (newest first)
- Pagination works (page 2 returns empty if only 2 bids)

---

### Test Scenario 7: Get Highest Bid

**Endpoint:** `GET /api/auctions/{auctionId}/bids/highest`

**Expected Response:** 200 OK
```json
{
  "id": "...",
  "auctionId": "550e8400-e29b-41d4-a716-446655440000",
  "bidderId": "b1000000-0000-0000-0000-000000000003",
  "amount": 115.00,
  "status": "Accepted",
  "placedAt": "2025-01-15T10:31:00Z",
  "isAutoBid": false,
  "bidder": { ... }
}
```

---

### Test Scenario 8: Concurrency Conflict (409 Conflict)

**Simulate concurrent bids** using async requests or a load testing tool:

1. Send two `POST /api/auctions/{auctionId}/bids` requests simultaneously
2. Both with different amounts but targeting the same auction

**Expected Behavior:**
- One succeeds: 201 Created
- One fails: 409 Conflict
```json
{
  "message": "Bid placement failed due to concurrent bidding activity. Please refresh the auction and try again."
}
```

**How to simulate in Postman:**
1. Create two requests (amount: 100, amount: 110)
2. Use Postman Runner or Newman CLI to send both rapidly
3. Or use `ab` (Apache Bench) or `wrk` for load testing

---

### Test Scenario 9: Invalid Request - Missing Amount

**Request:**
```json
{
  "amount": 0,
  "idempotencyKey": null
}
```

**Expected Response:** 400 Bad Request
```json
{
  "errors": [
	{
	  "propertyName": "Amount",
	  "errorMessage": "Bid amount must be greater than 0"
	}
  ]
}
```

---

### Test Scenario 10: Auction Not Found

**Endpoint:** `POST /api/auctions/00000000-0000-0000-0000-000000000000/bids`

**Expected Response:** 404 Not Found
```json
{
  "message": "Auction with ID 00000000-0000-0000-0000-000000000000 not found."
}
```

---

## Part 2: Database Verification

### After Each Bid Placement:

1. **Bids table:**
   ```sql
   SELECT * FROM Bids WHERE AuctionId = '550e8400-e29b-41d4-a716-446655440000'
   ORDER BY PlacedAt DESC;
   ```
   - Verify new row exists
   - Check RowVersion is not empty
   - Verify Status, Amount, PlacedAt

2. **Auctions table:**
   ```sql
   SELECT Id, CurrentHighestBid, Status FROM Auctions 
   WHERE Id = '550e8400-e29b-41d4-a716-446655440000';
   ```
   - Verify CurrentHighestBid updated to latest bid amount
   - Status still = Live

3. **Index usage (SQL Server Management Studio):**
   ```sql
   EXEC sp_helpindex 'Bids';
   ```
   - Verify indexes exist:
	 - IX_Bids_AuctionId
	 - IX_Bids_BidderId
	 - IX_Bids_PlacedAt
	 - IX_Bids_AuctionId_Amount_Desc

---

## Part 3: Concurrency Testing (Load Test)

### Using Apache Bench (ab):

```bash
# 100 concurrent requests, 500 total
ab -n 500 -c 100 -p bid_payload.json -T application/json \
  http://localhost:5000/api/auctions/550e8400-e29b-41d4-a716-446655440000/bids
```

**bid_payload.json:**
```json
{"amount": 150.00, "idempotencyKey": null}
```

**Expected Outcome:**
- Some requests succeed (201)
- Some fail with 409 Conflict (concurrency)
- All data remains consistent in database
- No duplicate winning bids at same amount
- CurrentHighestBid matches highest bid in table

### Using wrk:

```bash
wrk -t4 -c100 -d30s -s bid_load.lua http://localhost:5000/api/auctions/550e8400-e29b-41d4-a716-446655440000/bids
```

**bid_load.lua:**
```lua
request = function()
  wrk.method = "POST"
  wrk.body = '{"amount": ' .. math.random(100, 500) .. '.00}'
  wrk.headers["Content-Type"] = "application/json"
  return wrk.format(nil)
end
```

---

## Part 4: Data Integrity Checks

### Check for duplicate highest bids:

```sql
SELECT Amount, COUNT(*) as BidCount
FROM Bids
WHERE AuctionId = '550e8400-e29b-41d4-a716-446655440000'
  AND Status = 'Accepted'
GROUP BY Amount
HAVING COUNT(*) > 1;
```

**Expected:** Empty result set (no duplicates)

### Check for stale CurrentHighestBid:

```sql
SELECT a.Id, a.CurrentHighestBid, MAX(b.Amount) as ActualHighestBid
FROM Auctions a
LEFT JOIN Bids b ON a.Id = b.AuctionId AND b.Status = 'Accepted'
WHERE a.Id = '550e8400-e29b-41d4-a716-446655440000'
GROUP BY a.Id, a.CurrentHighestBid
HAVING a.CurrentHighestBid <> MAX(b.Amount);
```

**Expected:** Empty result set (no staleness)

### Check previous bid marking:

```sql
SELECT Status, COUNT(*) as BidCount
FROM Bids
WHERE AuctionId = '550e8400-e29b-41d4-a716-446655440000'
GROUP BY Status;
```

**Expected:** 
- Multiple `Accepted` and `Outbid` statuses
- No lost or invalid statuses

---

## Part 5: Known Limitations & TODOs

### Current Implementation Gaps:

1. **Authentication:**
   - BidsController uses hardcoded `bidderId` (TODO in code)
   - Replace with JWT claim extraction: `User.FindFirst(ClaimTypes.NameIdentifier)?.Value`

2. **SignalR Notifications:**
   - Currently using no-op service
   - Real-time updates not implemented yet (defer to Real-Time module)
   - Notifications not persisted for offline users

3. **Idempotency:**
   - Not yet implemented
   - Duplicate requests will create multiple bids
   - Defer to Step-5 implementation

4. **Rate Limiting:**
   - No protection against bid spam
   - Should implement per-user/IP rate limiting

5. **Anti-Sniping:**
   - No auction time extension on last-second bids
   - Can be added later if required

---

## Part 6: Success Criteria

✅ **Passed if:**
- Valid bids accepted and persisted correctly
- Invalid bids rejected with appropriate HTTP status
- Concurrency conflicts return 409 Conflict
- Previous winning bids marked as Outbid
- Auction.CurrentHighestBid never stale
- Bid history paginated correctly
- Load test shows no data corruption under 100+ concurrent requests
- Database indexes present and used

❌ **Failed if:**
- Duplicate accepted bids at same amount
- CurrentHighestBid not updated
- 409 errors not returned on concurrency
- 400 validation errors inconsistent
- Pagination returns wrong data

---

## Next Steps

1. **Manual Testing:** Execute scenarios 1-10 using Postman
2. **Load Testing:** Run concurrent bid simulation
3. **Data Verification:** Run integrity checks
4. **Real-Time Integration:** Implement SignalR notifications (Real-Time module)
5. **Authentication:** Extract bidderId from JWT claims
6. **Idempotency:** Implement idempotency cache (defer)

