using Nyumba_api.Models.DTOs.Bookings;

namespace Nyumba_api.Services.Bookings;

public interface IBookingService
{
    Task<BookingResponseDto> CreateAsync(CreateBookingDto dto, Guid userId);
    Task<List<BookingResponseDto>> GetMineAsync(Guid userId, int page, int pageSize);
    Task<List<BookingResponseDto>> GetForPropertyAsync(Guid propertyId, Guid userId, bool isAdmin, int page, int pageSize);
    Task<BookingResponseDto> UpdateStatusAsync(Guid bookingId, string status, Guid userId, bool isAdmin);
    Task<BookingResponseDto> CancelOwnBookingAsync(Guid bookingId, Guid userId);
}
