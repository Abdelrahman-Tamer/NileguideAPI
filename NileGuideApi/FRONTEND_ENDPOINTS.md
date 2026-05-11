# Frontend API Endpoints

This document covers the frontend-facing endpoints for activities, lookups, profile pictures, newsletter, wishlist, and plan.

Base URL depends on the environment. In local development it is usually:

```text
https://localhost:{port}
```

For protected endpoints, send:

```http
Authorization: Bearer {accessToken}
Content-Type: application/json
```

## Common Response Shapes

Validation errors return:

```json
{
  "message": "Validation failed",
  "errors": {
    "fieldName": "Validation error message"
  }
}
```

Simple success or error messages return:

```json
{
  "message": "Message text"
}
```

Paged responses return:

```json
{
  "totalCount": 25,
  "page": 1,
  "pageSize": 9,
  "items": []
}
```

## Auth And User Profile

### Auth Response

Returned by register, login, and refresh.

```ts
type AuthResponse = {
  token: string;
  expiresAtUtc: string;
  userId: number;
  role: string;
  dateOfBirth: string | null;
  age: number | null;
  profile_picture_url: string | null;
  refreshToken: string;
  refreshTokenExpiresAtUtc: string;
};
```

`dateOfBirth` uses `YYYY-MM-DD`. `age` is calculated by the API from `dateOfBirth`; do not send age from the frontend.

### User Profile

Returned by `GET /api/auth/me`.

```ts
type UserProfile = {
  userId: number;
  email: string;
  fullName: string;
  nationality: string;
  dateOfBirth: string | null;
  age: number | null;
  profile_picture_url: string | null;
  role: string;
};
```

### Register

Public endpoint. No token required.

```http
POST /api/auth/register
Content-Type: application/json
```

Body:

```json
{
  "email": "user@example.com",
  "password": "Password123",
  "fullName": "User Name",
  "nationality": "Egyptian",
  "dateOfBirth": "2000-05-20"
}
```

`dateOfBirth` is optional. If sent, it must not be in the future.

Success: `200 OK`

```json
{
  "token": "jwt-access-token",
  "expiresAtUtc": "2026-04-28T12:00:00Z",
  "userId": 1,
  "role": "Tourist",
  "dateOfBirth": "2000-05-20",
  "age": 25,
  "profile_picture_url": null,
  "refreshToken": "refresh-token",
  "refreshTokenExpiresAtUtc": "2026-05-28T12:00:00Z"
}
```

Errors:

- `400 Bad Request`: invalid email, password, fullName, nationality, or future dateOfBirth.
- `409 Conflict`: email already exists.
- `429 Too Many Requests`: register rate limit exceeded.

### Login

Public endpoint. No token required.

```http
POST /api/auth/login
Content-Type: application/json
```

Body:

```json
{
  "email": "user@example.com",
  "password": "Password123",
  "rememberMe": false
}
```

Success: `200 OK`

Returns `AuthResponse`.

Errors:

- `400 Bad Request`: invalid request body.
- `401 Unauthorized`: invalid credentials.
- `429 Too Many Requests`: login rate limit exceeded.

### Get Current User

Protected endpoint.

```http
GET /api/auth/me
Authorization: Bearer {accessToken}
```

Success: `200 OK`

Returns `UserProfile`.

Errors:

- `401 Unauthorized`: missing, invalid, or inactive user token.

## Activity Types

### ActivityCard

Used in activity lists and wishlist lists.

```ts
type ActivityCard = {
  activityID: number;
  activityName: string;
  description: string;
  categoryID: number;
  categoryName: string;
  cityID: number;
  cityName: string;
  minPrice: number;
  priceCurrency: string;
  imageUrl: string;
  requiredDocuments: string;
  isActive: boolean;
  rating: number;
  reviewsCount: number;
  providers: ActivityProvider[];
  openingHours: ActivityHour[];
};
```

### ActivityDetails

Used in the activity details page.

```ts
type ActivityDetails = {
  activityID: number;
  activityName: string;
  description: string;
  categoryID: number;
  categoryName: string;
  cityID: number;
  cityName: string;
  latitude: number;
  longitude: number;
  price: number;
  minPrice: number;
  priceCurrency: string;
  priceBasis: string;
  duration: number;
  groupSize: string;
  cancellation: string;
  requiredDocuments: string;
  isActive: boolean;
  images: string[];
  providers: ActivityProvider[];
  openingHours: ActivityHour[];
};
```

### Shared Activity Nested Types

```ts
type ActivityProvider = {
  providerName: string;
  link: string;
};

type ActivityHour = {
  day: string;
  openHour: number;
  openAmPm: "AM" | "PM";
  closeHour: number;
  closeAmPm: "AM" | "PM";
  openTime: string;
  closeTime: string;
};
```

## Activities

Public endpoints. No token required.

### Get Activities

```http
GET /api/activities
```

Gets a paged list of active activities. Inactive activities are hidden from public activity results.

Query params:

| Name | Type | Required | Default | Notes |
|---|---|---:|---:|---|
| categoryIds | number[] | no | none | Repeat the key for multiple values: `categoryIds=1&categoryIds=2` |
| cityIds | number[] | no | none | Repeat the key for multiple values: `cityIds=1&cityIds=2` |
| search | string | no | none | Max 100 characters |
| sortBy | string | no | default | `default`, `priceLowToHigh`, `priceHighToLow`, `name` |
| page | number | no | 1 | 1 to 10000 |
| pageSize | number | no | 9 | 1 to 50 |

Example:

```http
GET /api/activities?categoryIds=1&cityIds=2&search=nile&sortBy=priceLowToHigh&page=1&pageSize=9
```

Success: `200 OK`

```json
{
  "totalCount": 1,
  "page": 1,
  "pageSize": 9,
  "items": [
    {
      "activityID": 12,
      "activityName": "Nile Dinner Cruise",
      "description": "Evening cruise on the Nile",
      "categoryID": 2,
      "categoryName": "Cruises",
      "cityID": 1,
      "cityName": "Cairo",
      "minPrice": 40,
      "priceCurrency": "USD",
      "imageUrl": "https://example.com/image.jpg",
      "requiredDocuments": "",
      "isActive": true,
      "rating": 4.7,
      "reviewsCount": 120,
      "providers": [
        {
          "providerName": "Provider Name",
          "link": "https://example.com/book"
        }
      ],
      "openingHours": [
        {
          "day": "Daily",
          "openHour": 9,
          "openAmPm": "AM",
          "closeHour": 5,
          "closeAmPm": "PM",
          "openTime": "9:00 AM",
          "closeTime": "5:00 PM"
        }
      ]
    }
  ]
}
```

Errors:

- `400 Bad Request`: invalid filters, search length, sort value, page, or pageSize.

### Get Activity Details

```http
GET /api/activities/{id}
```

Gets one active activity by id.

Success: `200 OK`

```json
{
  "activityID": 12,
  "activityName": "Nile Dinner Cruise",
  "description": "Evening cruise on the Nile",
  "categoryID": 2,
  "categoryName": "Cruises",
  "cityID": 1,
  "cityName": "Cairo",
  "latitude": 30.0444,
  "longitude": 31.2357,
  "price": 50,
  "minPrice": 40,
  "priceCurrency": "USD",
  "priceBasis": "per person",
  "duration": 120,
  "groupSize": "1-10",
  "cancellation": "Free cancellation",
  "requiredDocuments": "",
  "isActive": true,
  "images": [
    "https://example.com/image-1.jpg",
    "https://example.com/image-2.jpg"
  ],
  "providers": [
    {
      "providerName": "Provider Name",
      "link": "https://example.com/book"
    }
  ],
  "openingHours": [
    {
      "day": "Daily",
      "openHour": 9,
      "openAmPm": "AM",
      "closeHour": 5,
      "closeAmPm": "PM",
      "openTime": "9:00 AM",
      "closeTime": "5:00 PM"
    }
  ]
}
```

Errors:

- `400 Bad Request`: id is zero or negative.
- `404 Not Found`: activity does not exist.

## Users

### Upload Profile Picture

Protected endpoint. Uploads the profile picture for the logged-in user.

```http
POST /api/users/me/profile-picture
Authorization: Bearer {accessToken}
Content-Type: multipart/form-data
```

Form data:

| Name | Type | Required | Notes |
|---|---|---:|---|
| image | File | yes | jpg, png, or webp only. Max 5MB. |

Success: `200 OK`

```json
{
  "profile_picture_url": "https://res.cloudinary.com/example/image/upload/v123/users/profile_pictures/file.jpg"
}
```

Errors:

- `400 Bad Request`: missing image, unsupported image type, or file larger than 5MB.
- `401 Unauthorized`: missing or invalid token.
- `404 Not Found`: user does not exist.
- `502 Bad Gateway`: Cloudinary upload failed.
- `503 Service Unavailable`: Cloudinary is not configured on the server.

## Categories

Public endpoint. No token required.

### Get Categories

```http
GET /api/categories
```

Success: `200 OK`

```json
[
  {
    "categoryID": 1,
    "categoryName": "Museums"
  },
  {
    "categoryID": 2,
    "categoryName": "Cruises"
  }
]
```

## Cities

Public endpoint. No token required.

### Get Cities

```http
GET /api/cities
```

Success: `200 OK`

```json
[
  {
    "cityID": 1,
    "cityName": "Cairo"
  },
  {
    "cityID": 2,
    "cityName": "Luxor"
  }
]
```

## Newsletter

### Subscribe

Public endpoint. No token required.

```http
POST /api/newsletter/subscribe
```

Body:

```json
{
  "email": "user@example.com"
}
```

Success: `200 OK`

Possible messages:

```json
{
  "message": "Subscribed successfully"
}
```

```json
{
  "message": "Email is already subscribed"
}
```

```json
{
  "message": "Subscription reactivated"
}
```

Errors:

- `400 Bad Request`: invalid or missing email.

### Unsubscribe

Public endpoint. No token required.

```http
POST /api/newsletter/unsubscribe
```

Body:

```json
{
  "email": "user@example.com"
}
```

Success: `200 OK`

```json
{
  "message": "If the email is subscribed, it has been unsubscribed."
}
```

Errors:

- `400 Bad Request`: invalid or missing email.

### Send Newsletter

Admin-only endpoint.

```http
POST /api/newsletter/send
Authorization: Bearer {adminAccessToken}
```

Body:

```json
{
  "subject": "New NileGuide updates",
  "body": "Newsletter content here"
}
```

Success: `200 OK`

```json
{
  "totalSubscribers": 100,
  "sentCount": 98,
  "failedCount": 2
}
```

Errors:

- `400 Bad Request`: missing subject/body or subject/body too long.
- `401 Unauthorized`: missing or invalid token.
- `403 Forbidden`: user is authenticated but not admin.

## Wishlist

All wishlist endpoints require a logged-in user.

### Get My Wishlist Activity IDs

```http
GET /api/wishlist/activity-ids
Authorization: Bearer {accessToken}
```

Gets all activity ids saved by the current user.

Use this endpoint after loading activities to mark saved heart icons in one request.

Success: `200 OK`

```json
[12, 18, 25]
```

Errors:

- `401 Unauthorized`: missing or invalid token.

### Get My Wishlist

```http
GET /api/wishlist?page=1&pageSize=9
Authorization: Bearer {accessToken}
```

Gets the current user's saved active activities, newest first. Saved activities that were disabled by admin are hidden from this response.

Query params:

| Name | Type | Required | Default | Notes |
|---|---|---:|---:|---|
| page | number | no | 1 | 1 to 10000 |
| pageSize | number | no | 9 | 1 to 50 |

Success: `200 OK`

```json
{
  "totalCount": 1,
  "page": 1,
  "pageSize": 9,
  "items": [
    {
      "activityID": 12,
      "activityName": "Nile Dinner Cruise",
      "description": "Evening cruise on the Nile",
      "categoryID": 2,
      "categoryName": "Cruises",
      "cityID": 1,
      "cityName": "Cairo",
      "minPrice": 40,
      "priceCurrency": "USD",
      "imageUrl": "https://example.com/image.jpg",
      "requiredDocuments": "",
      "isActive": true,
      "rating": 4.7,
      "reviewsCount": 120,
      "providers": [],
      "openingHours": []
    }
  ]
}
```

Errors:

- `400 Bad Request`: invalid page or pageSize.
- `401 Unauthorized`: missing or invalid token.

### Add Activity To Wishlist

```http
POST /api/wishlist/{activityId}
Authorization: Bearer {accessToken}
```

Adds an activity to the current user's wishlist.

Success: `200 OK`

If added:

```json
{
  "message": "Activity added to wishlist"
}
```

If already saved:

```json
{
  "message": "Activity already in wishlist"
}
```

Errors:

- `400 Bad Request`: activity id is zero or negative.
- `401 Unauthorized`: missing or invalid token.
- `404 Not Found`: activity does not exist.

### Remove Activity From Wishlist

```http
DELETE /api/wishlist/{activityId}
Authorization: Bearer {accessToken}
```

Removes an activity from the current user's wishlist.

This endpoint is idempotent. If the activity is not saved, it still returns success.

Success: `200 OK`

```json
{
  "message": "Activity removed from wishlist"
}
```

Errors:

- `400 Bad Request`: activity id is zero or negative.
- `401 Unauthorized`: missing or invalid token.

### Check Wishlist Status

```http
GET /api/wishlist/status/{activityId}
Authorization: Bearer {accessToken}
```

Checks if the current user saved an activity.

Success: `200 OK`

```json
{
  "activityID": 12,
  "isWishlisted": true
}
```

Errors:

- `400 Bad Request`: activity id is zero or negative.
- `401 Unauthorized`: missing or invalid token.
- `404 Not Found`: activity does not exist.

## Plan

All plan endpoints require a logged-in user. The backend stores the selected activity, date, and start time only. Conflict detection is handled by the frontend.

### Get My Plan

```http
GET /api/plan
Authorization: Bearer {accessToken}
```

Success: `200 OK`

```json
{
  "totalActivities": 2,
  "totalCost": 125,
  "priceCurrency": "USD",
  "items": [
    {
      "planItemId": 1,
      "activityId": 10,
      "scheduledDate": "2026-05-01",
      "startTime": "08:00",
      "endTime": "12:00",
      "activityName": "Pyramids of Giza Tour",
      "description": "Private guided tour with Egyptologist",
      "cityId": 2,
      "cityName": "Giza",
      "durationMinutes": 240,
      "price": 80,
      "priceCurrency": "USD",
      "isActive": true,
      "bookingLinks": [
        {
          "providerName": "Provider Name",
          "link": "https://example.com/book"
        }
      ]
    }
  ]
}
```

### Add Activity To Plan

```http
POST /api/plan/items
Authorization: Bearer {accessToken}
Content-Type: application/json
```

Body:

```json
{
  "activityId": 10,
  "scheduledDate": "2026-05-01",
  "startTime": "08:00"
}
```

Success: `200 OK`

Returns the added plan item. If the activity is already in the user's plan, the existing item is returned.

Errors:

- `400 Bad Request`: invalid activity id, scheduled date, or start time. `startTime` must be `HH:mm`.
- `401 Unauthorized`: missing or invalid token.
- `404 Not Found`: activity does not exist or is inactive.

### Remove Activity From Plan

```http
DELETE /api/plan/items/{planItemId}
Authorization: Bearer {accessToken}
```

Success: `204 No Content`

Errors:

- `401 Unauthorized`: missing or invalid token.
- `404 Not Found`: plan item does not exist or belongs to another user.

### Get My Planned Activity IDs

```http
GET /api/plan/activity-ids
Authorization: Bearer {accessToken}
```

Success: `200 OK`

```json
[10, 12, 15]
```

## Frontend Integration Notes

- Public browsing can use `Activities`, `Categories`, and `Cities` without a token.
- Recommended card flow: load activities, then call `GET /api/wishlist/activity-ids` once and store the returned ids in a `Set<number>`.
- Use `GET /api/wishlist/status/{activityId}` only when you need to check one activity directly, such as a details page refresh.
- Use `POST /api/wishlist/{activityId}` when the user clicks an empty heart.
- Use `DELETE /api/wishlist/{activityId}` when the user clicks a filled heart.
- Treat `"Activity already in wishlist"` as success.
- After successful delete, remove the item from UI even if it was already absent server-side.
- For the schedule page, call `GET /api/plan` and render the returned `items`.
- Use `POST /api/plan/items` when adding an activity to the schedule.
- Use `DELETE /api/plan/items/{planItemId}` when removing a row from the schedule.
- If a protected endpoint returns `401`, redirect to login or refresh the token through the auth flow.
