using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiCentral.Application.Queries;
using ApiCentral.Application.DTOs;

namespace ApiCentral.WebApi.Controllers;

[ApiController]
[Route("dashboards")]
[Authorize]
public class DashboardsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Métricas operacionais para gestão (Marcos Oliveira)
    /// </summary>
    [HttpGet("resumo")]
    public async Task<ActionResult<DashboardResumoDto>> GetResumo()
    {
        var query = new GetDashboardResumoQuery();
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }
}