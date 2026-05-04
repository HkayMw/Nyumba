using Nyumba_api.Services;
using Nyumba_api.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Nyumba_api.Services.Auth;
using Microsoft.AspNetCore.Identity;
using Nyumba_api.Models.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Nyumba_api.Infrastructure.Swagger;
using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;
using Nyumba_api.Infrastructure.Errors;
using Nyumba_api.Services.Bookings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    c.OperationFilter<AllowAnonymousOperationFilter>();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});
builder.Services.AddScoped<IPropertyService, PropertyService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<PasswordHasher<User>>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection missing");
var mysqlServerVersion = builder.Configuration["Database:ServerVersion"] ?? "8.0.36";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(Version.Parse(mysqlServerVersion))
    ));

var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt["Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
var issuer = jwt["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer missing");
var audience = jwt["Audience"] ?? throw new InvalidOperationException("Jwt:Audience missing");

if (key.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be at least 32 characters.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

        ValidateIssuer = true,
        ValidIssuer = issuer,

        ValidateAudience = true,
        ValidAudience = audience,

        ValidateLifetime = true,
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();


try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (builder.Configuration.GetValue("Database:ApplyMigrationsOnStartup", true))
    {
        db.Database.Migrate();
        Console.WriteLine("Database migrated successfully.");
    }

    if (builder.Configuration.GetValue("Database:SeedDataOnStartup", false))
    {
        var passwordHasher = scope.ServiceProvider.GetRequiredService<PasswordHasher<User>>();
        await AppDbSeeder.SeedAsync(db, passwordHasher);
        Console.WriteLine("Database seeded successfully.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Migration failed: {ex.Message}");
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var statusCode = exception switch
        {
            ApiException apiException => apiException.StatusCode,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        var title = statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "Unexpected error"
        };
        var detail = exception is ApiException or UnauthorizedAccessException or InvalidOperationException or ArgumentException
            || app.Environment.IsDevelopment()
                ? exception?.Message
                : "An unexpected error occurred.";

        context.Response.StatusCode = statusCode;
        await Results.Problem(
            title: title,
            detail: detail,
            statusCode: statusCode,
            instance: context.Request.Path).ExecuteAsync(context);
    });
});

// TODO: Remove Scalar in product
// Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
// Serve Swagger JSON at Scalar’s default location
app.UseSwagger(c =>
{
    // Serve v1 JSON at /openapi/v1
    c.RouteTemplate = "openapi/{documentName}.json";
});

// Scalar UI — will pick up /openapi/v1 automatically
app.MapScalarApiReference();
// }

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
