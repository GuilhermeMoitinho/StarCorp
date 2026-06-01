using Microsoft.AspNetCore.Mvc;
using StarCorp.Business.Dtos;
using StarCorp.Business.Notifications.Abstractions;
using StarCorp.Business.Services.Abstractions;
using StarCorp.WebApi.Abstractions;

namespace StarCorp.WebApi.Controllers;

public sealed class BookingsController(IBookingService bookings, INotificationContext notifications)
    : ApiController(notifications)
{
    [HttpPost]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request, CancellationToken ct)
    {
        var result = await bookings.CreateAsync(request, ct);
        if (Notifications.HasNotifications)
            return Respond();

        return CreatedAtAction(nameof(GetById), new { id = result!.Id }, result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(BookingResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await bookings.GetByIdAsync(id, ct);
        return Respond(result);
    }

    [HttpPost("{id:int}/payment")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Pay(int id, [FromBody] PaymentRequest request, CancellationToken ct)
    {
        var result = await bookings.PayAsync(id, request, ct);
        return Respond(result);
    }

    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(CancellationResponseDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        var result = await bookings.CancelAsync(id, ct);
        return Respond(result);
    }
}
