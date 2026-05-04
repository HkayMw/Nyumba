using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nyumba_api.Infrastructure.Auth;
using Nyumba_api.Models.DTOs.Bookings;
using Nyumba_api.Services.Bookings;

namespace Nyumba_api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Request a booking for an available property.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "User")]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateBookingDto dto)
    {
        var result = await _bookingService.CreateAsync(dto, User.GetUserId());
        return Ok(result);
    }

    /// <summary>
    /// List bookings made by the authenticated user.
    /// </summary>
    [HttpGet("mine")]
    [ProducesResponseType(typeof(List<BookingResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMine([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _bookingService.GetMineAsync(User.GetUserId(), page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// List bookings for a property owned by the authenticated landlord/agent or any property for admins.
    /// </summary>
    [HttpGet("property/{propertyId}")]
    [Authorize(Roles = "Landlord,Agent,Admin")]
    [ProducesResponseType(typeof(List<BookingResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForProperty(Guid propertyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _bookingService.GetForPropertyAsync(propertyId, User.GetUserId(), User.IsAdmin(), page, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Update booking status for an owned property booking.
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "Landlord,Agent,Admin")]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateBookingStatusDto dto)
    {
        var result = await _bookingService.UpdateStatusAsync(id, dto.Status, User.GetUserId(), User.IsAdmin());
        return Ok(result);
    }

    /// <summary>
    /// Cancel a booking made by the authenticated user.
    /// </summary>
    [HttpPut("{id}/cancel")]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var result = await _bookingService.CancelOwnBookingAsync(id, User.GetUserId());
        return Ok(result);
    }
}
