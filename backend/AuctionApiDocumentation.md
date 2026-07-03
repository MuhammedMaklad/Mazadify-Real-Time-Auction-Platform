# Mazadify API — Auction Feature

> Base URL: \http://localhost:5213\

---

## 1. Error Response Format

All errors return a consistent JSON shape:

**Non-validation errors (404, 400, 500):**
\\\json
{ "error": "Auction with ID ... not found.", "statusCode": 404, "details": null }
\\\

**Validation errors (400):**
\\\json
{
  "error": "Validation failed",
  "statusCode": 400,
  "details": [
    { "field": "Title", "message": "'Title' must not be empty." }
  ]
}
\\\

| HTTP Status | Meaning |
|---|---|
| 200 | Success |
| 201 | Created (POST) |
| 204 | No Content (DELETE) |
| 400 | Bad Request / Validation failed / Invalid operation |
| 404 | Resource not found |
| 500 | Internal server error |

---

## 2. Auctions

### GET /api/auctions — List auctions

Returns a paginated, filterable, searchable list of auctions.

**Query Parameters:**

| Parameter | Type | Required | Default | Description |
|---|---|---|---|---|
| \searchTerm\ | string | No | — | Free-text search in title and description |
| \categoryId\ | guid | No | — | Filter by category |
| \sellerId\ | guid | No | — | Filter by seller |
| \status\ | string | No | — | \Draft\, \Scheduled\, \Live\, \Ended\, \Cancelled\, \ReserveNotMet\ |
| \sortBy\ | string | No | \startTime\ | \startTime\, \ndTime\, \currentHighestBid\, \	itle\ |
| \sortDescending\ | bool | No | \alse\ | Sort direction |
| \page\ | int | No | \1\ | Page number (1-based) |
| \pageSize\ | int | No | \20\ | Items per page (max 100) |

**200 Response:**
\\\json
{
  "items": [
    {
      "id": "c1000000-0000-0000-0000-000000000003",
      "title": "Wireless Noise-Canceling Headphones",
      "startingPrice": 150.00,
      "reservePrice": 200.00,
      "currentHighestBid": 175.00,
      "bidIncrement": 10.00,
      "status": "Live",
      "deliveryType": "Shipping",
      "startTime": "2026-07-03T17:08:00Z",
      "endTime": "2026-07-09T17:08:00Z",
      "categoryName": "Audio & Headphones",
      "sellerUsername": "seller2",
      "itemCount": 1,
      "primaryImageUrl": "https://placehold.co/800x600?text=ANC+Headphones+-+Black"
    }
  ],
  "totalCount": 8,
  "page": 1,
  "pageSize": 20,
  "totalPages": 1
}
\\\

---

### GET /api/auctions/{id} — Get auction detail

Returns the full auction with category, seller, items, and images.

**Path Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| \id\ | guid | Yes | Auction ID |

**200 Response:**
\\\json
{
  "id": "c1000000-0000-0000-0000-000000000003",
  "title": "Wireless Noise-Canceling Headphones",
  "description": "Premium over-ear headphones with active noise cancellation...",
  "startingPrice": 150.00,
  "reservePrice": 200.00,
  "currentHighestBid": 175.00,
  "bidIncrement": 10.00,
  "status": "Live",
  "deliveryType": "Shipping",
  "deliveryNotes": null,
  "startTime": "2026-07-03T17:08:00Z",
  "endTime": "2026-07-09T17:08:00Z",
  "createdAt": "2026-07-02T17:08:00Z",
  "category": {
    "id": "a1000000-0000-0000-0000-000000000013",
    "name": "Audio & Headphones",
    "slug": "audio-headphones",
    "parentCategoryId": "a1000000-0000-0000-0000-000000000001"
  },
  "seller": {
    "id": "b1000000-0000-0000-0000-000000000003",
    "username": "seller2"
  },
  "items": [
    {
      "id": "d1000000-0000-0000-0000-000000000004",
      "name": "ANC Headphones - Black",
      "description": "Premium noise-canceling headphones with carrying case...",
      "condition": "Like New",
      "images": [
        {
          "id": "guid",
          "imageUrl": "https://placehold.co/800x600?text=ANC+Headphones+-+Black",
          "isPrimary": true,
          "displayOrder": 0
        },
        {
          "id": "guid",
          "imageUrl": "https://placehold.co/800x600?text=ANC+Headphones+-+Black+2",
          "isPrimary": false,
          "displayOrder": 1
        }
      ]
    }
  ]
}
\\\

**404 Response:**
\\\json
{ "error": "Auction with ID ... not found.", "statusCode": 404, "details": null }
\\\

---

### POST /api/auctions — Create auction

Creates a new auction in \Draft\ status.

**Request Body:**
\\\json
{
  "categoryId": "a1000000-0000-0000-0000-000000000014",
  "title": "Brand New Gaming Console",
  "description": "Next-gen gaming console, unopened box, sealed.",
  "startingPrice": 350.00,
  "reservePrice": 500.00,
  "bidIncrement": 20.00,
  "startTime": "2026-07-10T00:00:00Z",
  "endTime": "2026-07-17T00:00:00Z",
  "deliveryType": "Shipping",
  "deliveryNotes": null,
  "items": [
    {
      "name": "Gaming Console",
      "description": "Latest model with 1TB SSD",
      "condition": "New",
      "imageUrls": [
        "https://placehold.co/800x600?text=Console+Front"
      ]
    }
  ]
}
\\\

**Validation Rules:**

| Field | Rules |
|---|---|
| \categoryId\ | Required, must exist |
| \	itle\ | Required, max 200 characters |
| \description\ | Required, max 5000 characters |
| \startingPrice\ | Required, > 0 |
| \eservePrice\ | Required, >= 0 |
| \idIncrement\ | Required, > 0 |
| \startTime\ | Required, > current UTC time, < endTime |
| \ndTime\ | Required, > startTime |
| \deliveryType\ | Required, max 20 chars |
| \items\ | Required, at least 1 item |
| \items[].name\ | Required, max 200 chars |
| \items[].description\ | Required, max 2000 chars |
| \items[].condition\ | Required, max 30 chars |

**201 Response:** Returns the created \AuctionDto\ (same shape as GET detail) with \Location\ header pointing to \/api/auctions/{id}\.

**400 Response:** Validation errors or invalid operation.

**404 Response:** Category not found.

---

### PUT /api/auctions/{id} — Update auction

Updates an auction. Only allowed when status is \Draft\ or \Scheduled\. All fields are optional — only provided fields are updated.

**Path Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| \id\ | guid | Yes | Auction ID |

**Request Body (all fields optional, at least one required):**
\\\json
{
  "title": "Updated Title",
  "description": "Updated description...",
  "reservePrice": 600.00,
  "bidIncrement": 25.00,
  "startTime": "2026-07-11T00:00:00Z",
  "endTime": "2026-07-18T00:00:00Z",
  "deliveryType": "Pickup",
  "deliveryNotes": "New delivery instructions"
}
\\\

**200 Response:** Returns the updated \AuctionDto\.

**400 Response:** Invalid operation (auction not in Draft/Scheduled) or validation error.

**404 Response:** Auction not found.

---

### DELETE /api/auctions/{id} — Delete auction

Soft-deletes an auction. Only allowed when status is \Draft\. Sets \isDeleted = true\ and \deletedAt\ timestamp.

> **Note:** This is a soft delete — data remains in the database but is excluded from all queries via a global filter. There is no undo endpoint currently.

**Path Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| \id\ | guid | Yes | Auction ID |

**204 Response:** No content — deletion successful.

**400 Response:** Invalid operation (auction not in Draft status).

**404 Response:** Auction not found.

---

## 3. Categories

### GET /api/categories — Get category tree

Returns the full category hierarchy as a tree structure.

**200 Response:**
\\\json
[
  {
    "id": "a1000000-0000-0000-0000-000000000001",
    "name": "Electronics",
    "slug": "electronics",
    "children": [
      {
        "id": "a1000000-0000-0000-0000-000000000011",
        "name": "Computers",
        "slug": "computers",
        "children": []
      },
      {
        "id": "a1000000-0000-0000-0000-000000000012",
        "name": "Smartphones & Accessories",
        "slug": "smartphones-accessories",
        "children": []
      },
      {
        "id": "a1000000-0000-0000-0000-000000000013",
        "name": "Audio & Headphones",
        "slug": "audio-headphones",
        "children": []
      },
      {
        "id": "a1000000-0000-0000-0000-000000000014",
        "name": "Gaming",
        "slug": "gaming",
        "children": []
      }
    ]
  },
  {
    "id": "a1000000-0000-0000-0000-000000000002",
    "name": "Collectibles & Art",
    "slug": "collectibles-art",
    "children": [ ]
  },
  {
    "id": "a1000000-0000-0000-0000-000000000003",
    "name": "Vehicles",
    "slug": "vehicles",
    "children": [ ]
  },
  {
    "id": "a1000000-0000-0000-0000-000000000004",
    "name": "Home & Garden",
    "slug": "home-garden",
    "children": [ ]
  }
]
\\\

> **Note:** Child categories of subcategories are included recursively. The seed data has a 2-level hierarchy.

---

### GET /api/categories/{slug} — Get category by slug

Returns a single category looked up by its URL-friendly slug.

**Path Parameters:**

| Parameter | Type | Required | Description |
|---|---|---|---|
| \slug\ | string | Yes | URL slug (e.g., \lectronics\, \gaming\) |

**200 Response:**
\\\json
{
  "id": "a1000000-0000-0000-0000-000000000001",
  "name": "Electronics",
  "slug": "electronics",
  "parentCategoryId": null
}
\\\

**404 Response:**
\\\json
{ "error": "Category with slug '...' not found.", "statusCode": 404, "details": null }
\\\

---

## 4. Seed Data Reference (for testing)

### Category GUIDs

| Name | GUID | Parent |
|---|---|---|
| Electronics | \1000000-0000-0000-0000-000000000001\ | — |
| Collectibles & Art | \1000000-0000-0000-0000-000000000002\ | — |
| Vehicles | \1000000-0000-0000-0000-000000000003\ | — |
| Home & Garden | \1000000-0000-0000-0000-000000000004\ | — |
| Computers | \1000000-0000-0000-0000-000000000011\ | Electronics |
| Smartphones & Accessories | \1000000-0000-0000-0000-000000000012\ | Electronics |
| Audio & Headphones | \1000000-0000-0000-0000-000000000013\ | Electronics |
| Gaming | \1000000-0000-0000-0000-000000000014\ | Electronics |

### User GUIDs

| Username | GUID |
|---|---|
| admin | \1000000-0000-0000-0000-000000000001\ |
| seller1 | \1000000-0000-0000-0000-000000000002\ |
| seller2 | \1000000-0000-0000-0000-000000000003\ |

### Auction GUIDs (for direct testing)

| Title | GUID | Status |
|---|---|---|
| Vintage Camera Collection | \c1000000-0000-0000-0000-000000000001\ | Draft |
| Gaming Laptop Pro X | \c1000000-0000-0000-0000-000000000002\ | Scheduled |
| Wireless Noise-Canceling Headphones | \c1000000-0000-0000-0000-000000000003\ | Live |
| Antique Wooden Desk | \c1000000-0000-0000-0000-000000000004\ | Live |
| Classic Mustang 1967 | \c1000000-0000-0000-0000-000000000005\ | Ended |
| Latest Smartphone Bundle | \c1000000-0000-0000-0000-000000000006\ | ReserveNotMet |
| PS5 Pro + 5 Games Bundle | \c1000000-0000-0000-0000-000000000007\ | Live |
| Professional Tool Set | \c1000000-0000-0000-0000-000000000008\ | Scheduled |
