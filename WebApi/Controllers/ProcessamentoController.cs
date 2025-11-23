using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiCentral.Application.Queries;
using ApiCentral.Application.DTOs;

namespace ApiCentral.WebApi.Controllers;

[ApiController]
[Route("processamento")]
[Authorize]
public class ProcessamentoController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProcessamentoController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Lista os perfis disponíveis (ex.: Mala Direta)
    /// </summary>
    [HttpGet("perfis")]
    public async Task<ActionResult<List<PerfilProcessamentoDto>>> GetPerfis()
    {
        var query = new GetPerfilProcessamentoQuery();
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }
}