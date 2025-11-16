using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiCentral.Application.Queries;
using ApiCentral.Application.Commands;
using ApiCentral.Application.DTOs;
using ApiCentral.Application.Validators;
using FluentValidation;

namespace ApiCentral.WebApi.Controllers;

[ApiController]
[Route("lotes")]
[Authorize]
public class LotesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<UploadLoteRequest> _uploadLoteValidator;

    public LotesController(IMediator mediator, IValidator<UploadLoteRequest> uploadLoteValidator)
    {
        _mediator = mediator;
        _uploadLoteValidator = uploadLoteValidator;
    }

    /// <summary>
    /// Endpoint crítico - Recebe arquivos da Ana Ribeiro para processamento
    /// </summary>
    [HttpPost("upload")]
    public async Task<ActionResult<UploadLoteResponse>> UploadLote([FromBody] UploadLoteRequest request)
    {
        var validationResult = await _uploadLoteValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var userIdClaim = User.FindFirst("userId")?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var command = new UploadLoteCommand(
            request.ClienteId,
            request.PerfilProcessamentoId,
            request.ArquivoBase64,
            request.NomeArquivo,
            userId
        );

        var result = await _mediator.Send(command);
        
        return Ok(result);
    }

    /// <summary>
    /// Consulta detalhada do status e progresso do lote
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<LoteProcessamentoDto>> GetLote(int id)
    {
        var query = new GetLoteByIdQuery(id);
        var result = await _mediator.Send(query);
        
        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Listar lotes por cliente
    /// </summary>
    [HttpGet("cliente/{clienteId}")]
    public async Task<IActionResult> GetLotesByCliente(int clienteId)
    {
        var query = new GetLotesByClienteQuery(clienteId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obter logs de processamento de um lote
    /// </summary>
    [HttpGet("{id}/logs")]
    public async Task<IActionResult> GetLoteLogs(int id)
    {
        var query = new GetLoteLogsQuery(id);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}