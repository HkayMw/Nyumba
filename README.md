# Nyumba API

A modern RESTful backend service for accommodation booking and renting systems. Built with ASP.NET Core 8 and provides comprehensive APIs for managing properties, users, and authentication.

## Features

- **User Authentication & Authorization**
  - JWT-based authentication
  - User registration and login
  - Role-based access control (Admin, Landlord, Agent, Tenant)
  - Secure password hashing

- **Property Management**
  - Create and manage property listings
  - Advanced filtering and search capabilities
  - Pagination support
  - Comprehensive property details (price, bedrooms, bathrooms, square footage, location, etc.)
  - Property availability tracking

- **User Management**
  - Admin-controlled user creation
  - Role assignment and management
  - User data management

- **Security**
  - JWT token-based authentication
  - Role-based authorization on endpoints
  - Password hashing with EntityFramework Identity

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

Create or update `appsettings.json` with your database connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=nyumba_db;User=root;Password=your_password;"
  },
  "Jwt": {
    "Key": "your-long-random-secret-key-here-at-least-32-characters",
    "Issuer": "MyAppApi",
    "Audience": "MyAppApiUsers",
    "ExpiresMinutes": 60
  }
}
```

### 3. Build and Run

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`).

### 4. Apply Database Migrations

Migrations are automatically applied on startup. If you need to manually apply them:

```bash
dotnet ef database update
```

## API Documentation

Once running, access the interactive API documentation at:

- **Swagger UI**: `https://localhost:5001/swagger`
- **Scalar UI**: `https://localhost:5001/scalar/v1`

## API Endpoints

### Authentication Endpoints

- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Authenticate and receive JWT token

### Property Endpoints

- `POST /api/property` - Create a new property (Landlord, Agent only)
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
- **Tenant**: Can search and view property listings

## Environment Variables

Key configuration values in `appsettings.json`:

| Variable                              | Description                               | Required         |
| ------------------------------------- | ----------------------------------------- | ---------------- |
| `ConnectionStrings:DefaultConnection` | MySQL connection string                   | Yes              |
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

## Development

### Running Tests

```bash
dotnet test
```

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
- `500 Internal Server Error` - Server error

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

- [ ] Bookings and reservations system
- [ ] Property reviews and ratings
- [ ] Payment integration
- [ ] Notification system
- [ ] Advanced analytics and reporting
- [ ] Image upload and management for properties
