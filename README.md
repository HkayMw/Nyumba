# Nyumba API

A RESTful backend service for accommodation/property listing systems. Built with ASP.NET Core 8, it currently covers authentication, property listings, property search, and admin management.

## Features

- **User Authentication & Authorization**
  - JWT-based authentication
  - User registration and login
  - Role-based access control (Admin, Landlord, Agent, User)
  - Public registration cannot create Admin accounts
  - Secure password hashing

- **Property Management**
  - Create and manage property listings
  - Owner-only property update/delete for landlords and agents
  - Advanced filtering and search capabilities
  - Pagination support
  - Comprehensive property details (price, bedrooms, bathrooms, square footage, location, etc.)
  - Property availability tracking
  - Property image upload

- **User Management**
  - Admin-controlled user creation
  - Role assignment and management
  - User data management

- **Security**
  - JWT token-based authentication
  - Role-based authorization on endpoints
  - Password hashing with EntityFramework Identity
  - ProblemDetails error responses

- **Bookings**
  - Users can request bookings for available properties
  - Landlords/agents can view and manage bookings for their own properties
  - Admins can manage any booking

## Technology Stack

- **Framework**: ASP.NET Core 8
- **Database**: MySQL
- **ORM**: Entity Framework Core
- **Authentication**: JWT (JSON Web Tokens)
- **API Documentation**: Swagger/OpenAPI with Scalar UI
- **Password Hashing**: ASP.NET Core Identity

## Prerequisites

- .NET 8 SDK
- MySQL 5.7+ or MySQL 8.0+
- Git

## Getting Started

### 1. Clone the Repository

```bash
git clone <repository-url>
cd Nyumba_api
```

### 2. Configure Database Connection

Create or update `Nyumba_api/appsettings.json` and `Nyumba_api/appsettings.Development.json` with your database and JWT settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=nyumba_db;User=root;Password=your_password;"
  },
  "Database": {
    "ServerVersion": "8.0.36",
    "ApplyMigrationsOnStartup": true,
    "SeedDataOnStartup": false
  },
  "Jwt": {
    "Key": "your-long-random-secret-key-here-at-least-32-characters",
    "Issuer": "NyumbaApi",
    "Audience": "NyumbaApiUsers",
    "ExpiresMinutes": 60
  }
}
```

### 3. Build and Run

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build Nyumba_api.sln

# Run the application
dotnet run --project Nyumba_api/MyApp_api.csproj
```

The launch profile binds to `0.0.0.0`; access it locally at `http://127.0.0.1:5037` or `http://localhost:5037`.

### 4. Apply Database Migrations

Migrations are automatically applied on startup. If you need to manually apply them:

```bash
dotnet ef database update
```

## API Documentation

Once running, access the interactive API documentation at:

- **OpenAPI JSON**: `http://127.0.0.1:5037/openapi/v1.json`
- **Scalar UI**: `http://127.0.0.1:5037/scalar/v1`

## API Endpoints

### Authentication Endpoints

- `POST /api/auth/register` - Register a new Landlord, Agent, or User account
- `POST /api/auth/login` - Authenticate and receive JWT token

### Property Endpoints

- `POST /api/property` - Create a new property (Landlord, Agent only)
- `PUT /api/property/{id}` - Update an owned property (Landlord, Agent only)
- `DELETE /api/property/{id}` - Delete an owned property without active bookings (Landlord, Agent only)
- `POST /api/property/{id}/images` - Upload an image for an owned property
- `DELETE /api/property/{id}/images/{imageId}` - Remove an image from an owned property
- `GET /api/property` - Get all properties with optional filtering
  - Query Parameters:
    - `minPrice`, `maxPrice` - Price range filter
    - `title`, `district`, `city` - Location/name filters
    - `propertyType` - Filter by property type
    - `bedrooms`, `bathrooms` - Bedroom/bathroom count
    - `minSquareFeet`, `maxSquareFeet` - Size range
    - `page`, `pageSize` - Pagination (default: page=1, pageSize=20)
- `GET /api/property/{id}` - Get property details

### User Endpoints (Admin Only)

- `POST /api/user` - Create a new user
- `GET /api/user` - List all users

### Admin Endpoints (Admin Only)

- `GET /api/admin/users` - List users with pagination
- `PUT /api/admin/users/{id}/role` - Update a user role
- `DELETE /api/admin/users/{id}` - Delete a user without owned properties
- `GET /api/admin/properties` - List all properties, including unavailable
- `PUT /api/admin/properties/{id}/availability` - Update listing availability
- `DELETE /api/admin/properties/{id}` - Delete a property

### Booking Endpoints

- `POST /api/bookings` - Request a booking for an available property (User only)
- `GET /api/bookings/mine` - List bookings made by the authenticated user
- `GET /api/bookings/property/{propertyId}` - List bookings for an owned property (Landlord, Agent, Admin)
- `PUT /api/bookings/{id}/status` - Update booking status (Landlord, Agent, Admin)
- `PUT /api/bookings/{id}/cancel` - Cancel your own booking

## Project Structure

```
Nyumba_api/
├── Controllers/          # API endpoints
│   ├── Auth/
│   ├── Admin/
│   └── ...
├── Services/             # Business logic
├── Models/               # Data models
│   ├── DTOs/            # Data Transfer Objects
│   └── Entities/        # Database entities
├── Data/                 # Database context
├── Migrations/           # EF Core migrations
├── Infrastructure/       # Utilities and configurations
└── Program.cs           # Application startup
```

## Authentication

This API uses JWT (JSON Web Tokens) for authentication. Include the token in request headers:

```
Authorization: Bearer <your_jwt_token>
```

### User Roles

- **Admin**: Full system access, user management
- **Landlord**: Can create and manage property listings
- **Agent**: Can manage properties on behalf of landlords
- **User**: Can search and view property listings

Public registration allows `Landlord`, `Agent`, and `User`. Admin accounts must be created or promoted through trusted/admin-controlled flows.

## Environment Variables

Key configuration values in `appsettings.json`:

| Variable                              | Description                               | Required         |
| ------------------------------------- | ----------------------------------------- | ---------------- |
| `ConnectionStrings:DefaultConnection` | MySQL connection string                   | Yes              |
| `Database:ServerVersion`              | MySQL server version for EF configuration | No (8.0.36)      |
| `Database:ApplyMigrationsOnStartup`   | Automatically apply EF migrations         | No (true)        |
| `Database:SeedDataOnStartup`          | Seed sample users/properties on startup   | No (false)       |
| `Jwt:Key`                             | Secret key for JWT signing (min 32 chars) | Yes              |
| `Jwt:Issuer`                          | JWT issuer claim                          | Yes              |
| `Jwt:Audience`                        | JWT audience claim                        | Yes              |
| `Jwt:ExpiresMinutes`                  | Token expiration time in minutes          | No (default: 60) |

## Database Migrations

Create a new migration after model changes:

```bash
dotnet ef migrations add <MigrationName>
```

Update the database:

```bash
dotnet ef database update
```

After pulling booking or image changes, apply the latest migration:

```bash
dotnet ef database update --project Nyumba_api/MyApp_api.csproj --startup-project Nyumba_api/MyApp_api.csproj
```

## Development

### Seed Data

Set `Database:SeedDataOnStartup` to `true` for local development to create demo users and properties. The seeder is idempotent, so running it again will not duplicate the built-in records.

| Email                 | Password     | Role     |
| --------------------- | ------------ | -------- |
| `admin@nyumba.local`  | `Admin123!`  | Admin    |
| `landlord@nyumba.local` | `Password123!` | Landlord |
| `agent@nyumba.local`  | `Password123!` | Agent    |
| `user@nyumba.local`   | `Password123!` | User     |

Seeded properties include available listings in Lilongwe, Blantyre, and Mzuzu, plus one unavailable Blantyre listing for admin availability testing.

### Running Tests

```bash
dotnet test Nyumba_api.sln
```

### Manual API Smoke Test

Use [Nyumba_api/MyApp_api.http](Nyumba_api/MyApp_api.http) from your IDE, or run this flow:

```bash
dotnet run --project Nyumba_api/MyApp_api.csproj
```

1. `POST /api/auth/register` with role `Landlord`, `Agent`, or `User`.
2. `POST /api/auth/register` with role `Admin` and confirm it returns `400`.
3. `POST /api/auth/login` and copy the returned token.
4. `POST /api/property` with `Authorization: Bearer <token>` from a `Landlord` or `Agent`.
5. `GET /api/property?city=Lilongwe&district=Central` and confirm city and district filters both apply.
6. `PUT /api/property/{id}` as the owner and confirm non-owners receive `403`.
7. `POST /api/property/{id}/images` as multipart form data with `file` and optional `caption`.
8. `POST /api/bookings` as `user@nyumba.local`, then `PUT /api/bookings/{id}/status` as `landlord@nyumba.local`.

### Code Standards

- Nullable reference types are enabled
- Implicit usings are enabled
- Follow C# naming conventions

## Error Handling

The API returns appropriate HTTP status codes:

- `200 OK` - Successful request
- `201 Created` - Resource created successfully
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Authentication required or failed
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Duplicate resource, such as an existing email
- `500 Internal Server Error` - Server error

Unhandled application errors are returned as RFC 7807-style ProblemDetails JSON.

## Contributing

1. Create a feature branch (`git checkout -b feature/amazing-feature`)
2. Commit changes (`git commit -m 'Add amazing feature'`)
3. Push to the branch (`git push origin feature/amazing-feature`)
4. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues, questions, or suggestions, please open an issue in the repository.

## Future Enhancements

- [x] Booking/reservation request and owner status management
- [x] Property image upload and management
- [x] Ownership controls so landlords/agents can edit only their own listings
- [x] Global exception handling and structured problem responses
- [ ] Integration tests against a disposable database
- [ ] Availability calendar and booking date-rate rules
- [ ] Property reviews and ratings
- [ ] Payment integration
- [ ] Notification system
- [ ] Advanced analytics and reporting
