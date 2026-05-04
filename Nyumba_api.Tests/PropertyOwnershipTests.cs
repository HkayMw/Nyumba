using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Nyumba_api.Data;
using Nyumba_api.Infrastructure.Errors;
using Nyumba_api.Models.Authorization;
using Nyumba_api.Models.DTOs;
using Nyumba_api.Models.Entities;
using Nyumba_api.Services;
using Xunit;

namespace Nyumba_api.Tests;

public class PropertyOwnershipTests
{
    [Fact]
    public async Task UpdateAsyncAllowsOwnerToUpdateProperty()
    {
        await using var context = CreateContext();
        var property = await SeedPropertyAsync(context);
        var service = new PropertyService(context, new TestWebHostEnvironment());

        var result = await service.UpdateAsync(property.Id, new UpdatePropertyDto
        {
            Title = "Updated title",
            Price = 900,
            IsAvailable = true
        }, AppDbSeeder.LandlordUserId);

        Assert.Equal("Updated title", result.Title);
        Assert.Equal(900, result.Price);
    }

    [Fact]
    public async Task UpdateAsyncRejectsNonOwner()
    {
        await using var context = CreateContext();
        var property = await SeedPropertyAsync(context);
        var service = new PropertyService(context, new TestWebHostEnvironment());

        await Assert.ThrowsAsync<ForbiddenException>(() => service.UpdateAsync(property.Id, new UpdatePropertyDto
        {
            Title = "Nope",
            Price = 900,
            IsAvailable = true
        }, Guid.NewGuid()));
    }

    private static async Task<Property> SeedPropertyAsync(AppDbContext context)
    {
        var owner = new User
        {
            Id = AppDbSeeder.LandlordUserId,
            Email = "owner@example.com",
            Role = AppRoles.Landlord,
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = "Original title",
            Price = 700,
            IsAvailable = true,
            OwnerId = owner.Id,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(owner);
        context.Properties.Add(property);
        await context.SaveChangesAsync();

        return property;
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Nyumba_api.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public string EnvironmentName { get; set; } = "Development";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = Path.Combine(Path.GetTempPath(), "nyumba-api-tests-wwwroot");
    }
}
