using Microsoft.EntityFrameworkCore;
using Nyumba_api.Data;
using Nyumba_api.Infrastructure.Errors;
using Nyumba_api.Models.DTOs.Bookings;
using Nyumba_api.Models.Entities;

namespace Nyumba_api.Services.Bookings;

public class BookingService : IBookingService
{
    private readonly AppDbContext _context;

    public BookingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BookingResponseDto> CreateAsync(CreateBookingDto dto, Guid userId)
    {
        var startDate = dto.StartDate.Date;
        var endDate = dto.EndDate.Date;

        if (startDate >= endDate)
            throw new BadRequestException("End date must be after start date.");

        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == dto.PropertyId);
        if (property is null)
            throw new NotFoundException($"Property {dto.PropertyId} not found.");
        if (!property.IsAvailable)
            throw new ConflictException("Property is not available for booking.");
        if (property.OwnerId == userId)
            throw new BadRequestException("You cannot book your own property.");

        var overlaps = await HasActiveOverlapAsync(dto.PropertyId, startDate, endDate);
        if (overlaps)
            throw new ConflictException("Property already has a pending or confirmed booking for those dates.");

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            PropertyId = dto.PropertyId,
            UserId = userId,
            StartDate = startDate,
            EndDate = endDate,
            Status = BookingStatuses.Pending,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return await GetByIdAsDtoAsync(booking.Id);
    }

    public async Task<List<BookingResponseDto>> GetMineAsync(Guid userId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var bookings = await _context.Bookings
            .Include(b => b.Property)
            .Include(b => b.User)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return bookings.Select(MapToDto).ToList();
    }

    public async Task<List<BookingResponseDto>> GetForPropertyAsync(Guid propertyId, Guid userId, bool isAdmin, int page, int pageSize)
    {
        var property = await _context.Properties.FirstOrDefaultAsync(p => p.Id == propertyId);
        if (property is null)
            throw new NotFoundException($"Property {propertyId} not found.");
        if (!isAdmin && property.OwnerId != userId)
            throw new ForbiddenException("You can only view bookings for properties that you own.");

        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var bookings = await _context.Bookings
            .Include(b => b.Property)
            .Include(b => b.User)
            .Where(b => b.PropertyId == propertyId)
            .OrderByDescending(b => b.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return bookings.Select(MapToDto).ToList();
    }

    public async Task<BookingResponseDto> UpdateStatusAsync(Guid bookingId, string status, Guid userId, bool isAdmin)
    {
        var normalizedStatus = BookingStatuses.Normalize(status.Trim());
        if (!BookingStatuses.IsValid(normalizedStatus))
            throw new BadRequestException("Invalid booking status.");

        var booking = await _context.Bookings
            .Include(b => b.Property)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking is null)
            throw new NotFoundException($"Booking {bookingId} not found.");
        if (!isAdmin && booking.Property.OwnerId != userId)
            throw new ForbiddenException("You can only manage bookings for properties that you own.");

        if (normalizedStatus == BookingStatuses.Confirmed)
        {
            var overlaps = await HasActiveOverlapAsync(
                booking.PropertyId,
                booking.StartDate,
                booking.EndDate,
                excludeBookingId: booking.Id);
            if (overlaps)
                throw new ConflictException("Another pending or confirmed booking overlaps those dates.");
        }

        booking.Status = normalizedStatus;
        await _context.SaveChangesAsync();

        return MapToDto(booking);
    }

    public async Task<BookingResponseDto> CancelOwnBookingAsync(Guid bookingId, Guid userId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Property)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == bookingId);
        if (booking is null)
            throw new NotFoundException($"Booking {bookingId} not found.");
        if (booking.UserId != userId)
            throw new ForbiddenException("You can only cancel your own bookings.");

        booking.Status = BookingStatuses.Cancelled;
        await _context.SaveChangesAsync();

        return MapToDto(booking);
    }

    private async Task<bool> HasActiveOverlapAsync(
        Guid propertyId,
        DateTime startDate,
        DateTime endDate,
        Guid? excludeBookingId = null)
    {
        return await _context.Bookings.AnyAsync(b =>
            b.PropertyId == propertyId &&
            (!excludeBookingId.HasValue || b.Id != excludeBookingId.Value) &&
            (b.Status == BookingStatuses.Pending || b.Status == BookingStatuses.Confirmed) &&
            startDate < b.EndDate &&
            endDate > b.StartDate);
    }

    private async Task<BookingResponseDto> GetByIdAsDtoAsync(Guid bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Property)
            .Include(b => b.User)
            .FirstAsync(b => b.Id == bookingId);

        return MapToDto(booking);
    }

    private static BookingResponseDto MapToDto(Booking booking)
    {
        return new BookingResponseDto
        {
            Id = booking.Id,
            PropertyId = booking.PropertyId,
            PropertyTitle = booking.Property.Title,
            UserId = booking.UserId,
            UserEmail = booking.User.Email,
            StartDate = booking.StartDate,
            EndDate = booking.EndDate,
            Status = booking.Status,
            Notes = booking.Notes,
            CreatedAt = booking.CreatedAt
        };
    }
}
