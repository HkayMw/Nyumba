using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nyumba_api.Data;
using Nyumba_api.Models.Authorization;
using Nyumba_api.Models.Entities;
using Xunit;

namespace Nyumba_api.Tests;

public class AppDbSeederTests
{
    [Fact]
    public async Task SeedAsyncCreatesExpectedUsersAndProperties()
    {
        await using var context = CreateContext();
        var passwordHasher = new PasswordHasher<User>();

        await AppDbSeeder.SeedAsync(context, passwordHasher);

        Assert.Equal(4, await context.Users.CountAsync());
        Assert.Equal(4, await context.Properties.CountAsync());
        Assert.Equal(3, await context.Properties.CountAsync(p => p.IsAvailable));

        var admin = await context.Users.SingleAsync(u => u.Email == AppDbSeeder.AdminEmail);
        Assert.Equal(AppRoles.Admin, admin.Role);
        Assert.NotEqual(
            PasswordVerificationResult.Failed,
            passwordHasher.VerifyHashedPassword(admin, admin.PasswordHash, AppDbSeeder.AdminPassword));

        var landlord = await context.Users.SingleAsync(u => u.Email == "landlord@nyumba.local");
        Assert.Equal(2, await context.Properties.CountAsync(p => p.OwnerId == landlord.Id));
    }

    [Fact]
    public async Task SeedAsyncIsIdempotent()
    {
        await using var context = CreateContext();
        var passwordHasher = new PasswordHasher<User>();

        await AppDbSeeder.SeedAsync(context, passwordHasher);
        await AppDbSeeder.SeedAsync(context, passwordHasher);

        Assert.Equal(4, await context.Users.CountAsync());
        Assert.Equal(4, await context.Properties.CountAsync());
    }

    [Fact]
    public async Task SeedAsyncRepairsExistingSeedUserPasswordHash()
    {
        await using var context = CreateContext();
        var passwordHasher = new PasswordHasher<User>();
        var admin = new User
        {
            Id = AppDbSeeder.AdminUserId,
            Email = AppDbSeeder.AdminEmail,
            Role = AppRoles.User,
            PasswordHash = string.Empty,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        await AppDbSeeder.SeedAsync(context, passwordHasher);

        var seededAdmin = await context.Users.SingleAsync(u => u.Email == AppDbSeeder.AdminEmail);
        Assert.Equal(AppRoles.Admin, seededAdmin.Role);
        Assert.NotEqual(
            PasswordVerificationResult.Failed,
            passwordHasher.VerifyHashedPassword(seededAdmin, seededAdmin.PasswordHash, AppDbSeeder.AdminPassword));
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
