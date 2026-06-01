using Microsoft.AspNetCore.Mvc;
using StarCorp.Business.Dtos;
using StarCorp.Business.Notifications.Abstractions;
using StarCorp.Business.Services.Abstractions;
using StarCorp.Business.Pagination;
using StarCorp.WebApi.Abstractions;

namespace StarCorp.WebApi.Controllers;

public sealed class FlightsController(IFlightService flights, INotificationContext notifications)
    : ApiController(notifications)
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FlightOfferDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] FlightSearchRequest request, CancellationToken ct)
    {
        var result = await flights.SearchAsync(request, ct);
        return Respond(result);
    }
}
