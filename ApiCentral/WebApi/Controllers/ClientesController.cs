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
[Route("clientes")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateClienteRequest> _createClienteValidator;

    public ClientesController(IMediator mediator, IValidator<CreateClienteRequest> createClienteValidator)
    {
        _mediator = mediator;
        _createClienteValidator = createClienteValidator;
    }

    /// <summary>
    /// Lista todos os clientes cadastrados
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<object>> GetClientes()
    {
        var query = new GetClientesQuery();
        var clientes = await _mediator.Send(query);
        
        return Ok(new { clientes, total = clientes.Count });
    }

    /// <summary>
    /// Cadastra um novo cliente no sistema
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ClienteDto>> CreateCliente([FromBody] CreateClienteRequest request)
    {
        var validationResult = await _createClienteValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var command = new CreateClienteCommand(request.Nome, request.Email, request.Telefone, request.Empresa);
        var result = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetClientes), new { id = result.Id }, result);
    }
}