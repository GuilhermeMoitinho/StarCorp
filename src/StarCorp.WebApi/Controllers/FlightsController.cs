using Microsoft.AspNetCore.Mvc;
using StarCorp.Business.Dtos;
using StarCorp.Business.Notifications.Abstractions;
using StarCorp.Business.Services.Abstractions;
using StarCorp.Data.Pagination;
using StarCorp.WebApi.Abstractions;

namespace StarCorp.WebApi.Controllers;

public sealed class FlightsController(IFlightService flights, INotificationContext notifications)
    : ApiController(notifications)
{
    /// Busca voos com filtros opcionais (origem, destino, data, faixa de preco, classe) e paginacao.
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FlightOfferDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] FlightSearchRequest request, CancellationToken ct)
    {
        var result = await flights.SearchAsync(request, ct);
        return Respond(result);
    }
}
