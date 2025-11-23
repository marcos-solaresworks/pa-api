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

    /// <summary>
    /// Listar todos os lotes e seus status
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLotes()
    {
        var query = new GetLotesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Download do arquivo processado do lote
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadLote(int id)
    {
        // Buscar informações do lote
        var loteQuery = new GetLoteByIdQuery(id);
        var lote = await _mediator.Send(loteQuery);
        
        if (lote == null)
        {
            return NotFound("Lote não encontrado");
        }

        if (string.IsNullOrEmpty(lote.UrlArquivoProcessado))
        {
            return BadRequest("Arquivo processado não disponível");
        }

        try
        {
            // Se é uma URL pré-assinada, redireciona para ela
            if (lote.UrlArquivoProcessado.StartsWith("https://"))
            {
                return Redirect(lote.UrlArquivoProcessado);
            }

            // Se é um caminho S3, faz download via serviço
            var filePath = lote.UrlArquivoProcessado.Replace("s3://grafica-mvp-storage-qb1g7tq6/", "");
            var storageService = HttpContext.RequestServices.GetRequiredService<ApiCentral.Domain.Interfaces.IStorageService>();
            
            var fileBytes = await storageService.DownloadFileAsync(filePath);
            var fileName = System.IO.Path.GetFileName(filePath);
            
            // Determinar content type baseado na extensão
            var contentType = fileName.EndsWith(".pcl") ? "application/octet-stream" : 
                             fileName.EndsWith(".pdf") ? "application/pdf" : 
                             "application/octet-stream";

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro ao baixar arquivo: {ex.Message}");
        }
    }
}