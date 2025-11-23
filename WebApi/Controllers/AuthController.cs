using MediatR;
using Microsoft.AspNetCore.Mvc;
using ApiCentral.Application.Commands;
using ApiCentral.Application.DTOs;
using ApiCentral.Application.Validators;
using FluentValidation;

namespace ApiCentral.WebApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthController(IMediator mediator, IValidator<LoginRequest> loginValidator)
    {
        _mediator = mediator;
        _loginValidator = loginValidator;
    }

    /// <summary>
    /// Autentica operadores internos e retorna JWT access token
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var validationResult = await _loginValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors);
        }

        var command = new LoginCommand(request.Email, request.Senha);
        var result = await _mediator.Send(command);
        
        return Ok(result);
    }
}