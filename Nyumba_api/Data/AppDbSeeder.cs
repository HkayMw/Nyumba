using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nyumba_api.Models.Authorization;
using Nyumba_api.Models.Entities;

namespace Nyumba_api.Data;

public static class AppDbSeeder
{
    public const string AdminEmail = "admin@nyumba.local";
    public const string AdminPassword = "Admin123!";
    public const string DefaultPassword = "Password123!";

    internal static readonly Guid AdminUserId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    internal static readonly Guid LandlordUserId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    internal static readonly Guid AgentUserId = Guid.Parse("10000000-0000-0000-0000-000000000003");
    internal static readonly Guid RegularUserId = Guid.Parse("10000000-0000-0000-0000-000000000004");

    public static async Task SeedAsync(
        AppDbContext context,
        PasswordHasher<User> passwordHasher,
        CancellationToken cancellationToken = default)
    {
        var users = await EnsureUsersAsync(context, passwordHasher, cancellationToken);
        await EnsurePropertiesAsync(context, users, cancellationToken);
    }

    private static async Task<Dictionary<string, User>> EnsureUsersAsync(
        AppDbContext context,
        PasswordHasher<User> passwordHasher,
        CancellationToken cancellationToken)
    {
        var seedUsers = new[]
        {
            new SeedUser(AdminUserId, AdminEmail, AppRoles.Admin, AdminPassword),
            new SeedUser(LandlordUserId, "landlord@nyumba.local", AppRoles.Landlord, DefaultPassword),
            new SeedUser(AgentUserId, "agent@nyumba.local", AppRoles.Agent, DefaultPassword),
            new SeedUser(RegularUserId, "user@nyumba.local", AppRoles.User, DefaultPassword)
        };

        var emails = seedUsers.Select(u => u.Email).ToList();
        var existingUsers = await context.Users
            .Where(u => emails.Contains(u.Email))
            .ToDictionaryAsync(u => u.Email, StringComparer.OrdinalIgnoreCase, cancellationToken);

        foreach (var seedUser in seedUsers)
        {
            if (existingUsers.TryGetValue(seedUser.Email, out var existingUser))
            {
                existingUser.Role = seedUser.Role;

                var passwordResult = passwordHasher.VerifyHashedPassword(
                    existingUser,
                    existingUser.PasswordHash,
                    seedUser.Password);
                if (passwordResult == PasswordVerificationResult.Failed)
                    existingUser.PasswordHash = passwordHasher.HashPassword(existingUser, seedUser.Password);

                continue;
            }

            var user = new User
            {
                Id = seedUser.Id,
                Email = seedUser.Email,
                Role = seedUser.Role,
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = passwordHasher.HashPassword(user, seedUser.Password);

            context.Users.Add(user);
            existingUsers[seedUser.Email] = user;
        }

        await context.SaveChangesAsync(cancellationToken);
        return existingUsers;
    }

    private static async Task EnsurePropertiesAsync(
        AppDbContext context,
        IReadOnlyDictionary<string, User> users,
        CancellationToken cancellationToken)
    {
        var landlord = users["landlord@nyumba.local"];
        var agent = users["agent@nyumba.local"];

        var seedProperties = new[]
        {
            new SeedProperty(
                Guid.Parse("20000000-0000-0000-0000-000000000001"),
                landlord.Id,
                "Area 10 apartment",
                "Two bedroom apartment near shops and schools.",
                750m,
                "Area 10",
                "Lilongwe",
                "Central",
                "Apartment",
                2,
                1,
                850m,
                true),
            new SeedProperty(
                Guid.Parse("20000000-0000-0000-0000-000000000002"),
                landlord.Id,
                "Namiwawa family house",
                "Family house with a private yard and secure parking.",
                1200m,
                "Namiwawa",
                "Blantyre",
                "Southern",
                "House",
                4,
                3,
                1850m,
                true),
            new SeedProperty(
                Guid.Parse("20000000-0000-0000-0000-000000000003"),
                agent.Id,
                "Mzuzu compact studio",
                "Compact studio close to the main road.",
                450m,
                "City Centre",
                "Mzuzu",
                "Northern",
                "Studio",
                1,
                1,
                420m,
                true),
            new SeedProperty(
                Guid.Parse("20000000-0000-0000-0000-000000000004"),
                agent.Id,
                "Kanjedza townhouse",
                "Townhouse pending availability confirmation.",
                950m,
                "Kanjedza",
                "Blantyre",
                "Southern",
                "Townhouse",
                3,
                2,
                1300m,
                false)
        };

        var propertyIds = seedProperties.Select(p => p.Id).ToList();
        var existingPropertyIds = await context.Properties
            .Where(p => propertyIds.Contains(p.Id))
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);
        var existingPropertyIdSet = existingPropertyIds.ToHashSet();

        foreach (var seedProperty in seedProperties)
        {
            if (existingPropertyIdSet.Contains(seedProperty.Id))
                continue;

            context.Properties.Add(new Property
            {
                Id = seedProperty.Id,
                Title = seedProperty.Title,
                Description = seedProperty.Description,
                Price = seedProperty.Price,
                Address = seedProperty.Address,
                City = seedProperty.City,
                District = seedProperty.District,
                PropertyType = seedProperty.PropertyType,
                Bedrooms = seedProperty.Bedrooms,
                Bathrooms = seedProperty.Bathrooms,
                SquareFeet = seedProperty.SquareFeet,
                IsAvailable = seedProperty.IsAvailable,
                OwnerId = seedProperty.OwnerId,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private sealed record SeedUser(Guid Id, string Email, string Role, string Password);

    private sealed record SeedProperty(
        Guid Id,
        Guid OwnerId,
        string Title,
        string Description,
        decimal Price,
        string Address,
        string City,
        string District,
        string PropertyType,
        int Bedrooms,
        int Bathrooms,
        decimal SquareFeet,
        bool IsAvailable);
}
