# WebCMS API Endpoints - Agent Ready Guide

## Overview

The WebCMS API provides comprehensive RESTful endpoints for managing websites and menu items in the WebSpark.Portal application. When hosted at `https://webspark.markhazleton.com`, all API endpoints are accessible under the `/api/WebCMS/` path.

**Base URL**: `https://webspark.markhazleton.com/api/WebCMS/`

## Authentication & Security

- **Authentication**: Required for all endpoints via `[Authorize]` attribute
- **Content-Type**: `application/json`
- **Response Format**: JSON with standardized `ApiResponse<T>` wrapper
- **Area**: WebCMS (ASP.NET Core Areas routing)

## API Response Structure

### Success Response

```json
{
  "success": true,
  "data": {}, // Actual response data
  "message": "Operation completed successfully"
}
```

### Error Response

```json
{
  "success": false,
  "error": "Error type",
  "message": "Human readable error message",
  "validationErrors": {} // Optional validation errors dictionary
}
```

### Paginated Response

```json
{
  "success": true,
  "data": {
    "items": [], // Array of items
    "totalCount": 100,
    "page": 1,
    "pageSize": 20,
    "totalPages": 5
  },
  "message": "Retrieved X items"
}
```

## Website Management Endpoints

### 1. Get All Websites

**Endpoint**: `GET /api/WebCMS/websites`

**Description**: Retrieve all websites with optional filtering and pagination

**Query Parameters**:

- `page` (int, optional): Page number (1-based, default: 1)
- `pageSize` (int, optional): Items per page (1-100, default: 20)
- `search` (string, optional): Search term for website name or description
- `template` (string, optional): Filter by template type
- `isRecipeSite` (bool, optional): Filter by recipe site flag

**Example Request**:

```
GET https://webspark.markhazleton.com/api/WebCMS/websites?page=1&pageSize=10&search=portfolio&template=Modern
```

**Response Codes**:

- `200 OK`: Success with paginated website list
- `400 Bad Request`: Invalid parameters
- `500 Internal Server Error`: Server error

### 2. Get Website by ID

**Endpoint**: `GET /api/WebCMS/websites/{id}`

**Description**: Retrieve a specific website by its ID

**Path Parameters**:

- `id` (int, required): Website ID (must be > 0)

**Example Request**:

```
GET https://webspark.markhazleton.com/api/WebCMS/websites/1
```

**Response Codes**:

- `200 OK`: Website found and returned
- `404 Not Found`: Website not found
- `400 Bad Request`: Invalid ID
- `500 Internal Server Error`: Server error

### 3. Create Website

**Endpoint**: `POST /api/WebCMS/websites`

**Description**: Create a new website

**Request Body** (CreateWebsiteRequest):

```json
{
  "name": "My New Website", // Required, max 255 chars
  "description": "Website description", // Optional, max 1000 chars
  "siteTemplate": "Modern", // Optional, max 100 chars
  "siteStyle": "Blue", // Optional, max 100 chars
  "message": "Welcome message", // Optional
  "siteName": "mysite", // Optional, max 255 chars
  "websiteUrl": "https://example.com", // Optional, max 500 chars, must be valid URL
  "websiteTitle": "My Website Title", // Optional, max 255 chars
  "useBreadCrumbURL": false, // Boolean
  "isRecipeSite": false // Boolean
}
```

**Example Request**:

```
POST https://webspark.markhazleton.com/api/WebCMS/websites
Content-Type: application/json

{
  "name": "Portfolio Site",
  "description": "My professional portfolio",
  "siteTemplate": "Modern",
  "websiteUrl": "https://myportfolio.com",
  "useBreadCrumbURL": true,
  "isRecipeSite": false
}
```

**Response Codes**:

- `201 Created`: Website created successfully
- `400 Bad Request`: Validation errors
- `500 Internal Server Error`: Server error

### 4. Update Website

**Endpoint**: `PUT /api/WebCMS/websites/{id}`

**Description**: Update an existing website

**Path Parameters**:

- `id` (int, required): Website ID

**Request Body** (UpdateWebsiteRequest):

```json
{
  "name": "Updated Website Name", // Optional, max 255 chars
  "description": "Updated description", // Optional, max 1000 chars
  "siteTemplate": "Classic", // Optional, max 100 chars
  "siteStyle": "Green", // Optional, max 100 chars
  "message": "Updated message", // Optional
  "siteName": "updatedsite", // Optional, max 255 chars
  "websiteUrl": "https://updated.com", // Optional, max 500 chars, must be valid URL
  "websiteTitle": "Updated Title", // Optional, max 255 chars
  "useBreadCrumbURL": true, // Boolean
  "isRecipeSite": false // Boolean
}
```

**Example Request**:

```
PUT https://webspark.markhazleton.com/api/WebCMS/websites/1
Content-Type: application/json

{
  "name": "Updated Portfolio Site",
  "description": "My updated professional portfolio",
  "useBreadCrumbURL": false
}
```

**Response Codes**:

- `200 OK`: Website updated successfully
- `400 Bad Request`: Invalid ID or validation errors
- `404 Not Found`: Website not found
- `500 Internal Server Error`: Server error

### 5. Delete Website

**Endpoint**: `DELETE /api/WebCMS/websites/{id}`

**Description**: Delete a website

**Path Parameters**:

- `id` (int, required): Website ID

**Example Request**:

```
DELETE https://webspark.markhazleton.com/api/WebCMS/websites/1
```

**Response Codes**:

- `200 OK`: Website deleted successfully
- `400 Bad Request`: Invalid ID
- `404 Not Found`: Website not found
- `500 Internal Server Error`: Server error

## Menu Management Endpoints

### 6. Get All Menu Items

**Endpoint**: `GET /api/WebCMS/menus`

**Description**: Retrieve all menu items with optional filtering and pagination

**Query Parameters**:

- `page` (int, optional): Page number (1-based, default: 1)
- `pageSize` (int, optional): Items per page (1-100, default: 20)
- `search` (string, optional): Search term for menu title or description
- `domainId` (int, optional): Filter by domain/website ID
- `parentId` (int, optional): Filter by parent menu ID
- `displayInNavigation` (bool, optional): Filter by navigation visibility

**Example Request**:

```
GET https://webspark.markhazleton.com/api/WebCMS/menus?domainId=1&displayInNavigation=true&page=1&pageSize=50
```

**Response Codes**:

- `200 OK`: Success with paginated menu list
- `400 Bad Request`: Invalid parameters
- `500 Internal Server Error`: Server error

### 7. Get Menu Item by ID

**Endpoint**: `GET /api/WebCMS/menus/{id}`

**Description**: Retrieve a specific menu item by its ID

**Path Parameters**:

- `id` (int, required): Menu item ID

**Example Request**:

```
GET https://webspark.markhazleton.com/api/WebCMS/menus/5
```

**Response Codes**:

- `200 OK`: Menu item found and returned (MenuEditModel)
- `404 Not Found`: Menu item not found
- `400 Bad Request`: Invalid ID
- `500 Internal Server Error`: Server error

### 8. Create Menu Item

**Endpoint**: `POST /api/WebCMS/menus`

**Description**: Create a new menu item

**Request Body** (CreateMenuItemRequest):

```json
{
  "domainID": 1, // Required, website ID
  "title": "About Us", // Required, max 255 chars
  "icon": "fa fa-info", // Optional, max 50 chars
  "pageContent": "<p>About us content</p>", // Optional, HTML content
  "action": "About", // Optional, max 100 chars
  "apiUrl": "https://api.example.com/about", // Optional, max 500 chars
  "argument": "about-us", // Optional, max 255 chars
  "controller": "Page", // Optional, max 100 chars
  "description": "About us page description", // Optional, max 1000 chars
  "displayInNavigation": true, // Boolean, default true
  "displayOrder": 10, // Integer >= 0
  "parentId": null, // Optional, parent menu ID
  "url": "about-us" // Optional, max 255 chars (auto-generated if empty)
}
```

**Example Request**:

```
POST https://webspark.markhazleton.com/api/WebCMS/menus
Content-Type: application/json

{
  "domainID": 1,
  "title": "Services",
  "icon": "fa fa-cogs",
  "controller": "Page",
  "action": "Services",
  "description": "Our services page",
  "displayInNavigation": true,
  "displayOrder": 20
}
```

**Response Codes**:

- `201 Created`: Menu item created successfully
- `400 Bad Request`: Validation errors
- `500 Internal Server Error`: Server error

### 9. Update Menu Item

**Endpoint**: `PUT /api/WebCMS/menus/{id}`

**Description**: Update an existing menu item

**Path Parameters**:

- `id` (int, required): Menu item ID

**Request Body** (UpdateMenuItemRequest):

```json
{
  "title": "Updated Services", // Optional, max 255 chars
  "icon": "fa fa-tools", // Optional, max 50 chars
  "pageContent": "<p>Updated content</p>", // Optional
  "action": "UpdatedServices", // Optional, max 100 chars
  "apiUrl": "https://api.example.com/services", // Optional, max 500 chars
  "argument": "updated-services", // Optional, max 255 chars
  "controller": "Page", // Optional, max 100 chars
  "description": "Updated description", // Optional, max 1000 chars
  "displayInNavigation": false, // Boolean
  "displayOrder": 25, // Integer >= 0
  "parentId": 2, // Optional, parent menu ID
  "url": "updated-services" // Optional, max 255 chars
}
```

**Example Request**:

```
PUT https://webspark.markhazleton.com/api/WebCMS/menus/5
Content-Type: application/json

{
  "title": "Professional Services",
  "description": "Our professional consulting services",
  "displayOrder": 15
}
```

**Response Codes**:

- `200 OK`: Menu item updated successfully
- `400 Bad Request`: Invalid ID or validation errors
- `404 Not Found`: Menu item not found
- `500 Internal Server Error`: Server error

### 10. Delete Menu Item

**Endpoint**: `DELETE /api/WebCMS/menus/{id}`

**Description**: Delete a menu item (also handles child menu orphaning)

**Path Parameters**:

- `id` (int, required): Menu item ID

**Example Request**:

```
DELETE https://webspark.markhazleton.com/api/WebCMS/menus/5
```

**Response Codes**:

- `200 OK`: Menu item deleted successfully
- `400 Bad Request`: Invalid ID
- `404 Not Found`: Menu item not found or deletion failed
- `500 Internal Server Error`: Server error

## Utility and Analytics Endpoints

### 11. Get Dashboard Statistics

**Endpoint**: `GET /api/WebCMS/dashboard/stats`

**Description**: Retrieve dashboard summary statistics

**Example Request**:

```
GET https://webspark.markhazleton.com/api/WebCMS/dashboard/stats
```

**Response Structure**:

```json
{
  "success": true,
  "data": {
    "totalWebsites": 5,
    "totalMenuItems": 25,
    "recentWebsites": [], // Last 5 websites
    "recentMenuItems": [], // Last 5 menu items
    "websitesByTemplate": {
      "Modern": 3,
      "Classic": 2
    },
    "menuItemsByDomain": {
      "Portfolio": 10,
      "Blog": 15
    }
  }
}
```

**Response Codes**:

- `200 OK`: Statistics retrieved successfully
- `500 Internal Server Error`: Server error

### 12. Get Menu Hierarchy

**Endpoint**: `GET /api/WebCMS/websites/{websiteId}/menu-hierarchy`

**Description**: Retrieve hierarchical menu structure for a specific website

**Path Parameters**:

- `websiteId` (int, required): Website ID

**Example Request**:

```
GET https://webspark.markhazleton.com/api/WebCMS/websites/1/menu-hierarchy
```

**Response Structure**:

```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "title": "Home",
      "icon": "fa fa-home",
      "url": "home",
      "displayOrder": 1,
      "displayInNavigation": true,
      "children": [
        {
          "id": 2,
          "title": "About",
          "icon": "fa fa-info",
          "url": "about",
          "displayOrder": 1,
          "displayInNavigation": true,
          "children": []
        }
      ]
    }
  ]
}
```

**Response Codes**:

- `200 OK`: Menu hierarchy retrieved successfully
- `404 Not Found`: Website not found
- `500 Internal Server Error`: Server error

### 13. Bulk Update Menu Order

**Endpoint**: `PUT /api/WebCMS/menus/bulk-update-order`

**Description**: Update display order for multiple menu items in bulk

**Request Body** (BulkUpdateOrderRequest):

```json
{
  "menuOrders": [
    {
      "menuId": 1, // Required
      "displayOrder": 10 // Required, >= 0
    },
    {
      "menuId": 2,
      "displayOrder": 20
    }
  ]
}
```

**Example Request**:

```
PUT https://webspark.markhazleton.com/api/WebCMS/menus/bulk-update-order
Content-Type: application/json

{
  "menuOrders": [
    {"menuId": 1, "displayOrder": 1},
    {"menuId": 2, "displayOrder": 2},
    {"menuId": 3, "displayOrder": 3}
  ]
}
```

**Response Codes**:

- `200 OK`: Orders updated successfully
- `400 Bad Request`: Invalid request or empty menu orders
- `500 Internal Server Error`: Server error

### 14. Global Search

**Endpoint**: `GET /api/WebCMS/search`

**Description**: Search across all content (websites and menus)

**Query Parameters**:

- `query` (string, required): Search query
- `page` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Items per page (1-100, default: 20)

**Example Request**:

```
GET https://webspark.markhazleton.com/api/WebCMS/search?query=portfolio&page=1&pageSize=10
```

**Response Structure**:

```json
{
  "success": true,
  "data": {
    "query": "portfolio",
    "items": [
      {
        "id": 1,
        "type": "Website",
        "title": "Portfolio Site",
        "description": "My professional portfolio",
        "url": "/WebCMS/Website/Details/1",
        "lastModified": "2024-01-15T10:30:00Z",
        "parentInfo": null
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1,
    "websiteCount": 1,
    "menuItemCount": 0
  }
}
```

**Response Codes**:

- `200 OK`: Search completed successfully
- `400 Bad Request`: Empty or invalid query
- `500 Internal Server Error`: Server error

## Curl Examples

### Website Management Endpoints

#### 1. Get All Websites

```bash
# Basic request - get all websites
curl -X GET "https://webspark.markhazleton.com/api/WebCMS/websites" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"

# With pagination and filtering
curl -X GET "https://webspark.markhazleton.com/api/WebCMS/websites?page=1&pageSize=10&search=portfolio&template=Modern&isRecipeSite=false" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

#### 2. Get Website by ID

```bash
curl -X GET "https://webspark.markhazleton.com/api/WebCMS/websites/1" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

#### 3. Create Website

```bash
curl -X POST "https://webspark.markhazleton.com/api/WebCMS/websites" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "name": "Portfolio Site",
    "description": "My professional portfolio",
    "siteTemplate": "Modern",
    "siteStyle": "Blue",
    "websiteUrl": "https://myportfolio.com",
    "websiteTitle": "My Portfolio",
    "useBreadCrumbURL": true,
    "isRecipeSite": false
  }'
```

#### 4. Update Website

```bash
# Partial update - only update specific fields
curl -X PUT "https://webspark.markhazleton.com/api/WebCMS/websites/1" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "name": "Updated Portfolio Site",
    "description": "My updated professional portfolio",
    "useBreadCrumbURL": false
  }'

# Full update
curl -X PUT "https://webspark.markhazleton.com/api/WebCMS/websites/1" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "name": "Complete Portfolio Site",
    "description": "My complete professional portfolio",
    "siteTemplate": "Classic",
    "siteStyle": "Green",
    "message": "Welcome to my portfolio",
    "siteName": "portfolio",
    "websiteUrl": "https://portfolio.example.com",
    "websiteTitle": "John Doe Portfolio",
    "useBreadCrumbURL": true,
    "isRecipeSite": false
  }'
```

#### 5. Delete Website

```bash
curl -X DELETE "https://webspark.markhazleton.com/api/WebCMS/websites/1" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Menu Management Endpoints

#### 6. Get All Menu Items

```bash
# Basic request - get all menu items
curl -X GET "https://webspark.markhazleton.com/api/WebCMS/menus" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"

# With filtering by domain and navigation visibility
curl -X GET "https://webspark.markhazleton.com/api/WebCMS/menus?domainId=1&displayInNavigation=true&page=1&pageSize=50" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"

# Search menu items
curl -X GET "https://webspark.markhazleton.com/api/WebCMS/menus?search=about&parentId=0" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

#### 7. Get Menu Item by ID

```bash
curl -X GET "https://webspark.markhazleton.com/api/WebCMS/menus/5" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

#### 8. Create Menu Item

```bash
# Create top-level menu item
curl -X POST "https://webspark.markhazleton.com/api/WebCMS/menus" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "domainID": 1,
    "title": "Services",
    "icon": "fa fa-cogs",
    "controller": "Page",
    "action": "Services",
    "description": "Our services page",
    "displayInNavigation": true,
    "displayOrder": 20
  }'

# Create sub-menu item with parent
curl -X POST "https://webspark.markhazleton.com/api/WebCMS/menus" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "domainID": 1,
    "title": "Web Development",
    "icon": "fa fa-code",
    "parentId": 2,
    "controller": "Page",
    "action": "WebDevelopment",
    "argument": "web-development",
    "description": "Professional web development services",
    "pageContent": "<p>We offer comprehensive web development services...</p>",
    "displayInNavigation": true,
    "displayOrder": 10,
    "url": "services/web-development"
  }'
```

#### 9. Update Menu Item

```bash
# Partial update
curl -X PUT "https://webspark.markhazleton.com/api/WebCMS/menus/5" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "title": "Professional Services",
    "description": "Our professional consulting services",
    "displayOrder": 15
  }'

# Full update with content
curl -X PUT "https://webspark.markhazleton.com/api/WebCMS/menus/5" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "title": "Updated Services",
    "icon": "fa fa-tools",
    "pageContent": "<p>Updated service content with HTML formatting</p>",
    "action": "UpdatedServices",
    "apiUrl": "https://api.example.com/services",
    "argument": "updated-services",
    "controller": "Page",
    "description": "Comprehensive updated description",
    "displayInNavigation": true,
    "displayOrder": 25,
    "parentId": 2,
    "url": "updated-services"
  }'
```

#### 10. Delete Menu Item

```bash
curl -X DELETE "https://webspark.markhazleton.com/api/WebCMS/menus/5" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Utility and Analytics Endpoints

#### 11. Get Dashboard Statistics

```bash
curl -X GET "https://webspark.markhazleton.com/api/WebCMS/dashboard/stats" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

#### 12. Get Menu Hierarchy

```bash
curl -X GET "https://webspark.markhazleton.com/api/WebCMS/websites/1/menu-hierarchy" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

#### 13. Bulk Update Menu Order

```bash
# Update display order for multiple menu items
curl -X PUT "https://webspark.markhazleton.com/api/WebCMS/menus/bulk-update-order" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -d '{
    "menuOrders": [
      {"menuId": 1, "displayOrder": 1},
      {"menuId": 2, "displayOrder": 2},
      {"menuId": 3, "displayOrder": 3},
      {"menuId": 4, "displayOrder": 4}
    ]
  }'
```

#### 14. Global Search

```bash
# Basic search
curl -X GET "https://webspark.markhazleton.com/api/WebCMS/search?query=portfolio" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"

# Search with pagination
curl -X GET "https://webspark.markhazleton.com/api/WebCMS/search?query=portfolio&page=1&pageSize=10" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"

# URL encoded search for special characters
curl -X GET "https://webspark.markhazleton.com/api/WebCMS/search?query=web%20development&page=1&pageSize=20" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Advanced Curl Examples

#### Authentication Examples

```bash
# Using cookie authentication (if using ASP.NET Core Identity)
curl -X GET "https://webspark.markhazleton.com/api/WebCMS/websites" \
  -H "Accept: application/json" \
  -b "cookies.txt"

# Using session authentication
curl -X GET "https://webspark.markhazleton.com/api/WebCMS/websites" \
  -H "Accept: application/json" \
  -H "Cookie: .AspNetCore.Session=YOUR_SESSION_ID"
```

#### Error Handling Examples

```bash
# Save response to file for debugging
curl -X GET "https://webspark.markhazleton.com/api/WebCMS/websites/999" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -o response.json \
  -w "HTTP Status: %{http_code}\n"

# Show response headers and status
curl -X GET "https://webspark.markhazleton.com/api/WebCMS/websites" \
  -H "Accept: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  -i -v
```

#### Batch Operations Example

```bash
#!/bin/bash
# Create multiple menu items for a website

WEBSITE_ID=1
BASE_URL="https://webspark.markhazleton.com/api/WebCMS"
AUTH_HEADER="Authorization: Bearer YOUR_TOKEN_HERE"

# Create Home menu
curl -X POST "$BASE_URL/menus" \
  -H "Content-Type: application/json" \
  -H "$AUTH_HEADER" \
  -d "{\"domainID\": $WEBSITE_ID, \"title\": \"Home\", \"displayOrder\": 1}"

# Create About menu
curl -X POST "$BASE_URL/menus" \
  -H "Content-Type: application/json" \
  -H "$AUTH_HEADER" \
  -d "{\"domainID\": $WEBSITE_ID, \"title\": \"About\", \"displayOrder\": 2}"

# Create Services menu
curl -X POST "$BASE_URL/menus" \
  -H "Content-Type: application/json" \
  -H "$AUTH_HEADER" \
  -d "{\"domainID\": $WEBSITE_ID, \"title\": \"Services\", \"displayOrder\": 3}"
```

## Data Models

### WebsiteModel Properties

- `Id` (int): Website ID
- `Name` (string): Website name
- `Description` (string): Website description
- `SiteTemplate` (string): Template type
- `SiteStyle` (string): Style theme
- `Message` (string): Welcome message
- `SiteName` (string): Site folder name
- `WebsiteUrl` (string): External URL
- `WebsiteTitle` (string): HTML title
- `UseBreadCrumbURL` (bool): Breadcrumb navigation flag
- `IsRecipeSite` (bool): Recipe site flag
- `ModifiedDT` (DateTime): Last modified date
- `ModifiedID` (int): Last modifier ID
- `VersionNo` (int): Version number

### MenuModel Properties

- `Id` (int): Menu item ID
- `DomainID` (int): Associated website ID
- `Title` (string): Menu title
- `Icon` (string): Font Awesome icon class
- `PageContent` (string): HTML content
- `Action` (string): Controller action
- `ApiUrl` (string): API endpoint URL
- `Argument` (string): Route argument
- `Controller` (string): Controller name
- `Description` (string): Menu description
- `DisplayInNavigation` (bool): Show in navigation
- `DisplayOrder` (int): Sort order
- `ParentId` (int?): Parent menu ID
- `Url` (string): URL slug
- `VirtualPath` (string): Virtual path
- `LastModified` (DateTime): Last modified date

## Error Handling

The API uses consistent error handling with the following HTTP status codes:

- **200 OK**: Successful GET, PUT operations
- **201 Created**: Successful POST operations
- **400 Bad Request**: Invalid input, validation errors
- **401 Unauthorized**: Authentication required
- **403 Forbidden**: Access denied
- **404 Not Found**: Resource not found
- **500 Internal Server Error**: Server-side errors

All error responses include:

- `success: false`
- `error`: Error type/category
- `message`: Human-readable error description
- `validationErrors`: Field-specific validation errors (when applicable)

## Rate Limiting & Performance

- **Pagination**: All list endpoints support pagination (max 100 items per page)
- **Filtering**: Search and filter capabilities to reduce payload size
- **Caching**: Response caching implemented for better performance
- **Async Operations**: All database operations are asynchronous

## Example Use Cases

### 1. Create a Complete Website with Menu Structure

```javascript
// 1. Create website
const website = await fetch('/api/WebCMS/websites', {
  method: 'POST',
  headers: {'Content-Type': 'application/json'},
  body: JSON.stringify({
    name: 'My Portfolio',
    description: 'Professional portfolio site',
    siteTemplate: 'Modern'
  })
});

// 2. Create main menu items
const homeMenu = await fetch('/api/WebCMS/menus', {
  method: 'POST',
  headers: {'Content-Type': 'application/json'},
  body: JSON.stringify({
    domainID: website.data.id,
    title: 'Home',
    controller: 'Page',
    action: 'Index',
    displayOrder: 1
  })
});
```

### 2. Get Website with Full Menu Hierarchy

```javascript
// Get website details
const website = await fetch('/api/WebCMS/websites/1');

// Get menu hierarchy
const menuHierarchy = await fetch('/api/WebCMS/websites/1/menu-hierarchy');
```

### 3. Search and Filter Operations

```javascript
// Search across all content
const searchResults = await fetch('/api/WebCMS/search?query=portfolio&page=1&pageSize=20');

// Filter menus by domain
const domainMenus = await fetch('/api/WebCMS/menus?domainId=1&displayInNavigation=true');
```

## Authentication Requirements

All endpoints require authentication. Ensure your requests include:

- Valid authentication cookies/tokens
- Proper CORS headers if calling from a different domain
- Content-Type: application/json for POST/PUT requests

## API Documentation

When running in development mode, API documentation is available at:

- **OpenAPI Specification**: `https://webspark.markhazleton.com/openapi/v1.json`
- **Scalar API UI**: `https://webspark.markhazleton.com/scalar/v1`

This comprehensive API enables full content management system functionality for websites and their navigation structures, with proper validation, error handling, and performance optimizations built-in.
