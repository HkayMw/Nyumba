# JWT Auth Implementation Walkthrough (Service-First)

This guide is written for your current structure (interfaces + service implementations holding business logic).

Goal:
- `POST /api/auth/register`
- `POST /api/auth/login`
- JWT validation in middleware
- `PasswordHash` persisted in DB
- protected property creation using claims user ID

---

## 0. Current gaps to fix first

Before anything else, these are blocking:
1. `AuthController` is incomplete and breaks build.
2. `AuthService` has `_configuration` and `_passwordHasher` fields but constructor does not initialize them.
3. `GenerateToken(user)` is called but not implemented.
4. `Program.cs` has `UseAuthentication()` but no `AddAuthentication().AddJwtBearer(...)`.
5. `PasswordHash` exists in model, but migration/snapshot has not been updated.

---

## 1. Keep architecture clean (who owns what)

Use this split:
- `AuthController`: thin transport layer only (HTTP concerns).
- `IAuthService/AuthService`: auth business logic (register, login, hashing, token generation).
- `Program.cs`: DI + middleware pipeline + JWT token validation config.

If logic includes email normalization, password hashing, credential validation, claim creation, token creation: it belongs in `AuthService`.

---

## 2. Complete the contracts (DTO + service interface)

Files:
- `MyApp_api/Models/DTOs/Auth/RegisterDto.cs`
- `MyApp_api/Models/DTOs/Auth/LoginDto.cs`
- `MyApp_api/Models/DTOs/Auth/AuthResponseDto.cs`
- `MyApp_api/Services/Auth/IAuthService.cs`

Checklist:
1. Keep methods in `IAuthService`:
   - `Task<AuthResponseDto> RegisterAsync(RegisterDto dto);`
   - `Task<AuthResponseDto> LoginAsync(LoginDto dto);`
2. Add validation attributes to DTOs:
   - `Email`: `[Required]`, `[EmailAddress]`
   - `Password`: `[Required]`, `[MinLength(...)]`
   - `Role`: `[Required]`, regex or controlled allowed values.
3. `AuthResponseDto` should at least return:
   - `Token`
   - `ExpiresAt`

Why: your controller gets automatic model validation from `[ApiController]` and stays lean.

---

## 3. Implement `AuthService` fully

File:
- `MyApp_api/Services/Auth/AuthService.cs`

### 3.1 Constructor injection
Inject and assign:
1. `AppDbContext context`
2. `IConfiguration configuration`
3. `PasswordHasher<User> passwordHasher` (or instantiate in constructor)

Store them in private readonly fields.

### 3.2 Register flow
Inside `RegisterAsync`:
1. Normalize email (`Trim().ToLower()`).
2. Check uniqueness in DB.
3. Validate role (if not already strictly controlled by DTO).
4. Create `User`.
5. Hash password into `user.PasswordHash`.
6. Save user.
7. Generate JWT token.
8. Return `AuthResponseDto`.

### 3.3 Login flow
Inside `LoginAsync`:
1. Normalize email.
2. Load user by email.
3. Verify hashed password.
4. If invalid, return generic auth error.
5. Generate token and return response.

### 3.4 Add `GenerateToken(User user)` private method
Use:
1. `JwtSecurityTokenHandler`
2. `ClaimTypes.NameIdentifier` = `user.Id`
3. `ClaimTypes.Email` = `user.Email`
4. `ClaimTypes.Role` = `user.Role`
5. signing key from config `Jwt:Key`
6. issuer/audience from config
7. expiry from config

Return token string and align returned `ExpiresAt`.

---

## 4. Finish `AuthController`

File:
- `MyApp_api/Controllers/Auth/AuthController.cs`

Route choice (recommended):
- `[Route("api/auth")]`

Endpoints:
1. `[HttpPost("register")]` -> calls `_authService.RegisterAsync(dto)`
2. `[HttpPost("login")]` -> calls `_authService.LoginAsync(dto)`

Controller rules:
- No hashing logic.
- No JWT creation logic.
- No DB queries directly.

Use `try/catch` only if you want custom status messages; otherwise let global exception handling manage errors.

---

## 5. Configure JWT authentication in `Program.cs`

File:
- `MyApp_api/Program.cs`

### 5.1 Add required namespaces
Add JWT/auth namespaces (`JwtBearer`, `TokenValidationParameters`, `SymmetricSecurityKey`, `Encoding`).

### 5.2 Register auth services
Before `builder.Build()`:
1. Read `Jwt` section from configuration.
2. `builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(...)`
3. Set validation parameters:
   - `ValidateIssuerSigningKey = true`
   - `ValidateIssuer = true`
   - `ValidateAudience = true`
   - `ValidateLifetime = true`
4. `builder.Services.AddAuthorization();`

### 5.3 Middleware order
Keep:
1. `app.UseAuthentication();`
2. `app.UseAuthorization();`

Order matters.

---

## 6. Add JWT config values

Files:
- `MyApp_api/appsettings.json` (local real config)
- optionally mirror in `appsettings.json.example` and `appsettings.json.template`

Add a `Jwt` section:
- `Key`
- `Issuer`
- `Audience`
- `ExpiresMinutes`

Use a long random key (at least 32+ chars for HMAC secret).

---

## 7. Update DB schema for `PasswordHash`

You already added `PasswordHash` to `User`, now persist it.

Commands:
```bash
dotnet ef migrations add AddPasswordHashToUser --project MyApp_api/MyApp_api.csproj
dotnet ef database update --project MyApp_api/MyApp_api.csproj
```

Verify migration includes new `Users.PasswordHash` column.

---

## 8. Protect business endpoint and use claims

File:
- `MyApp_api/Controllers/PropertyController.cs`

Steps:
1. Add `[Authorize]` to `Create` endpoint (or controller).
2. Remove hardcoded GUID.
3. Read user ID from `ClaimTypes.NameIdentifier`.
4. Parse to `Guid`, return `Unauthorized()` if missing/invalid.
5. Pass parsed `userId` into `_service.CreateAsync(dto, userId)`.

---

## 9. Verify end-to-end

Run:
```bash
dotnet build MyApp_api.sln
dotnet run --project MyApp_api/MyApp_api.csproj
```

Test flow:
1. `POST /api/auth/register` with email/password/role
2. `POST /api/auth/login` -> get token
3. `POST /api/property` with `Authorization: Bearer <token>`
4. confirm created property owner is authenticated user

If step 3 fails with 401:
1. check JWT `issuer/audience/key` match between token creation and token validation
2. check `UseAuthentication()` exists and is before `UseAuthorization()`
3. check `[Authorize]` is on the endpoint

---

## 10. Practical completion checklist

- [ ] `AuthController` compiles and has both endpoints
- [ ] `AuthService` constructor initializes all dependencies
- [ ] `GenerateToken` implemented
- [ ] JWT bearer configured in DI
- [ ] `Jwt` section exists in config
- [ ] DB migration created/applied for `PasswordHash`
- [ ] property create endpoint uses claim user ID (no hardcoded GUID)
- [ ] build passes
- [ ] register/login/property-create tested manually

