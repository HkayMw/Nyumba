using Microsoft.EntityFrameworkCore;
using Nyumba_api.Data;
using Nyumba_api.Infrastructure.Errors;
using Nyumba_api.Models.Authorization;
using Nyumba_api.Models.DTOs.Bookings;
using Nyumba_api.Models.Entities;
using Nyumba_api.Services.Bookings;
using Xunit;

namespace Nyumba_api.Tests;

public class BookingServiceTests
{
    [Fact]
    public async Task CreateAsyncCreatesPendingBookingForAvailableProperty()
    {
        await using var context = CreateContext();
        var property = await SeedUsersAndPropertyAsync(context);
        var service = new BookingService(context);

        var result = await service.CreateAsync(new CreateBookingDto
        {
            PropertyId = property.Id,
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 5),
            Notes = "Viewing trip"
        }, AppDbSeeder.RegularUserId);

        Assert.Equal(BookingStatuses.Pending, result.Status);
        Assert.Equal(property.Id, result.PropertyId);
        Assert.Equal(AppDbSeeder.RegularUserId, result.UserId);
    }

    [Fact]
    public async Task CreateAsyncRejectsOverlappingActiveBooking()
    {
        await using var context = CreateContext();
        var property = await SeedUsersAndPropertyAsync(context);
        var service = new BookingService(context);

        await service.CreateAsync(new CreateBookingDto
        {
            PropertyId = property.Id,
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 5)
        }, AppDbSeeder.RegularUserId);

        await Assert.ThrowsAsync<ConflictException>(() => service.CreateAsync(new CreateBookingDto
        {
            PropertyId = property.Id,
            StartDate = new DateTime(2026, 6, 3),
            EndDate = new DateTime(2026, 6, 7)
        }, Guid.NewGuid()));
    }

    private static async Task<Property> SeedUsersAndPropertyAsync(AppDbContext context)
    {
        var owner = new User
        {
            Id = AppDbSeeder.LandlordUserId,
            Email = "owner@example.com",
            Role = AppRoles.Landlord,
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        var guest = new User
        {
            Id = AppDbSeeder.RegularUserId,
            Email = "guest@example.com",
            Role = AppRoles.User,
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        var otherGuest = new User
        {
            Id = Guid.NewGuid(),
            Email = "other@example.com",
            Role = AppRoles.User,
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        var property = new Property
        {
            Id = Guid.NewGuid(),
            Title = "Test property",
            Price = 600,
            IsAvailable = true,
            OwnerId = owner.Id,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.AddRange(owner, guest, otherGuest);
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
}
