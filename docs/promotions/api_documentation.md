# Promotions API Documentation

## Overview

The Promotions API provides endpoints for managing promotional campaigns within the koi pond construction and maintenance system. Promotions can be created with specific discount percentages, validity periods, and targeting.

## Base URL

```
/api/Promotions
```

## Authentication

All endpoints require authentication with a valid JWT token provided in the Authorization header.

```
Authorization: Bearer <your_token>
```

## Endpoint Details

### 1. Get All Promotions

Retrieves a paginated list of promotions with optional filtering.

**HTTP Request**

```
GET /api/Promotions
```

**Query Parameters**

| Parameter   | Type          | Required | Description                                   |
|-------------|---------------|----------|-----------------------------------------------|
| Search      | string        | No       | Filter by promotion name                      |
| Status      | string        | No       | Filter by promotion status (PENDING, ACTIVE, EXPIRED) |
| Discount    | integer       | No       | Filter by specific discount percentage        |
| StartAt     | datetime      | No       | Filter by start date (after this date)        |
| ExpiredAt   | datetime      | No       | Filter by expiration date (before this date)  |
| IsActive    | boolean       | No       | Filter by active status                       |
| PageNumber  | integer       | No       | Page number (default: 1)                      |
| PageSize    | integer       | No       | Number of items per page (default: 10)        |
| SortColumn  | string        | No       | Column to sort by                             |
| SortDir     | string        | No       | Sort direction (Asc or Desc)                  |

**Response**

Status Code: 200 OK

```json
{
  "data": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Summer Sale",
      "code": "SUMMER25",
      "description": "25% off on all pond services during summer",
      "discount": 25,
      "startAt": "2023-06-01T00:00:00Z",
      "expiredAt": "2023-08-31T23:59:59Z",
      "status": "ACTIVE",
      "isActive": true,
      "createdAt": "2023-05-15T09:30:00Z",
      "updatedAt": "2023-05-15T09:30:00Z"
    },
    // Additional promotion items...
  ],
  "total": 10,
  "pageNumber": 1,
  "pageSize": 10
}
```

### 2. Get Promotion By ID

Retrieves a specific promotion by its unique identifier.

**HTTP Request**

```
GET /api/Promotions/{id}
```

**Path Parameters**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id        | guid | Yes      | Unique identifier of the promotion |

**Response**

Status Code: 200 OK

```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "Summer Sale",
    "code": "SUMMER25",
    "description": "25% off on all pond services during summer",
    "discount": 25,
    "startAt": "2023-06-01T00:00:00Z",
    "expiredAt": "2023-08-31T23:59:59Z",
    "status": "ACTIVE",
    "isActive": true,
    "createdAt": "2023-05-15T09:30:00Z",
    "updatedAt": "2023-05-15T09:30:00Z"
  }
}
```

**Error Responses**

| Status Code | Description | Possible Cause |
|-------------|-------------|----------------|
| 404 Not Found | Promotion not found | The specified promotion ID does not exist |
| 400 Bad Request | Promotion is not active | The promotion exists but is inactive |

### 3. Create Promotion

Creates a new promotion with the specified details.

**HTTP Request**

```
POST /api/Promotions
```

**Request Body**

```json
{
  "name": "Winter Sale",
  "code": "WINTER20",
  "description": "20% discount on all pond heating systems",
  "discount": 20,
  "startAt": "2023-12-01T00:00:00Z",
  "expiredAt": "2024-01-31T23:59:59Z",
  "isActive": true
}
```

| Field       | Type     | Required | Description                                    |
|-------------|----------|----------|------------------------------------------------|
| name        | string   | Yes      | Name of the promotion                          |
| code        | string   | No       | Promotion code (auto-generated if not provided)|
| description | string   | No       | Detailed description of the promotion          |
| discount    | integer  | Yes      | Discount percentage (1-100)                    |
| startAt     | datetime | Yes      | Start date and time of the promotion           |
| expiredAt   | datetime | Yes      | End date and time of the promotion             |
| isActive    | boolean  | No       | Whether the promotion is active (default: true)|

**Response**

Status Code: 200 OK

```json
{
  "success": true,
  "message": "Promotion created successfully"
}
```

**Error Responses**

| Status Code | Description | Possible Cause |
|-------------|-------------|----------------|
| 400 Bad Request | Validation error | Invalid input data such as negative discount, start date after end date, etc. |
| 400 Bad Request | Code already exists | A promotion with the specified code already exists |

**Business Logic**

- If the code is not provided, a random 6-character alphanumeric code will be generated
- The promotion status is automatically determined based on the start and expiration dates:
  - If `startAt` is in the future: Status = PENDING
  - If current date is between `startAt` and `expiredAt`: Status = ACTIVE
  - If `expiredAt` is in the past: Status = EXPIRED
- Background jobs are scheduled to automatically:
  - Activate the promotion when the start date is reached
  - Expire the promotion when the end date is reached

### 4. Update Promotion

Updates an existing promotion with new details.

**HTTP Request**

```
PUT /api/Promotions/{id}
```

**Path Parameters**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id        | guid | Yes      | Unique identifier of the promotion to update |

**Request Body**

```json
{
  "name": "Updated Winter Sale",
  "code": "WINTER20",
  "description": "Updated description: 20% discount on all pond heating systems",
  "discount": 20,
  "startAt": "2023-12-01T00:00:00Z",
  "expiredAt": "2024-02-28T23:59:59Z",
  "isActive": true
}
```

| Field       | Type     | Required | Description                                    |
|-------------|----------|----------|------------------------------------------------|
| name        | string   | Yes      | Updated name of the promotion                  |
| code        | string   | Yes      | Promotion code (must be unique)                |
| description | string   | No       | Updated description of the promotion           |
| discount    | integer  | Yes      | Updated discount percentage (1-100)            |
| startAt     | datetime | Yes      | Updated start date and time                    |
| expiredAt   | datetime | Yes      | Updated end date and time                      |
| isActive    | boolean  | No       | Whether the promotion is active                |

**Response**

Status Code: 200 OK

```json
{
  "success": true,
  "message": "Promotion updated successfully"
}
```

**Error Responses**

| Status Code | Description | Possible Cause |
|-------------|-------------|----------------|
| 404 Not Found | Promotion not found | The specified promotion ID does not exist |
| 400 Bad Request | Validation error | Invalid input data |
| 400 Bad Request | Code conflict | The updated code conflicts with another promotion |

**Business Logic**

- The promotion status is recalculated based on the updated start and expiration dates
- Background jobs for activation and expiration are rescheduled if needed
- All date fields are properly converted to the database timezone

### 5. Delete Promotion

Deletes a promotion or marks it as inactive if it's being used by quotations.

**HTTP Request**

```
DELETE /api/Promotions/{id}
```

**Path Parameters**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| id        | guid | Yes      | Unique identifier of the promotion to delete |

**Response**

Status Code: 200 OK

```json
{
  "success": true,
  "message": "Promotion deleted successfully"
}
```

**Error Responses**

| Status Code | Description | Possible Cause |
|-------------|-------------|----------------|
| 404 Not Found | Promotion not found | The specified promotion ID does not exist |

**Business Logic**

- If the promotion is referenced by any quotations, it will be marked as inactive (soft delete) instead of being physically removed
- If the promotion is not referenced anywhere, it will be completely removed from the database

## Status Codes

The API uses the following status codes:

| Status Code | Description |
|-------------|-------------|
| 200 OK | The request was successful |
| 400 Bad Request | The request was invalid or cannot be served |
| 401 Unauthorized | Authentication is required and has failed or not been provided |
| 403 Forbidden | The authenticated user does not have permission to access the requested resource |
| 404 Not Found | The requested resource does not exist |
| 500 Internal Server Error | An unexpected condition was encountered on the server |

## Date Format

All dates should be submitted and are returned in ISO 8601 format:

```
YYYY-MM-DDThh:mm:ssZ
```

Example: `2023-06-01T00:00:00Z` represents June 1, 2023, at midnight UTC.

## Promotion Statuses

A promotion can have one of the following statuses:

| Status | Description |
|--------|-------------|
| PENDING | The promotion is created but not yet active (start date is in the future) |
| ACTIVE | The promotion is currently active (current date is between start and end dates) |
| EXPIRED | The promotion has ended (end date is in the past) |

## Implementation Notes

- The system automatically manages status changes through background jobs
- Timezone handling is properly implemented with conversion to SEA time (Asia/Ho_Chi_Minh)
- The database handles common fields like CreatedAt and UpdatedAt automatically
- The promotion code is auto-generated if not provided during creation

## Koi Pond Construction Context

Promotions are an important part of the Koi Pond Construction and Operation System, providing the ability to:

1. Offer seasonal discounts on koi pond construction services
2. Provide special promotions for maintenance packages
3. Create limited-time offers for premium koi pond designs
4. Manage promotional campaigns for equipment and supplies

Typical promotions in the koi pond construction business might include:

- Seasonal discounts during low-demand periods
- Special offers for returning customers
- Bundled discounts when ordering both construction and maintenance services
- Holiday promotions for koi pond accessories and equipment

The promotion system helps the business attract customers and increase sales while providing customers with opportunities to save on high-quality koi pond services. 