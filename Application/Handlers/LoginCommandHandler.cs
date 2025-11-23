using MediatR;
using ApiCentral.Application.Commands;
using ApiCentral.Application.DTOs;
using ApiCentral.Domain.Interfaces;
using ApiCentral.Domain.Exceptions;

namespace ApiCentral.Application.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        IUsuarioRepository usuarioRepository,
        IPasswordService passwordService,
        IJwtService jwtService)
    {
        _usuarioRepository = usuarioRepository;
        _passwordService = passwordService;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(request.Email);
        
        if (usuario == null || !_passwordService.VerifyPassword(request.Senha, usuario.SenhaHash))
        {
            throw new UnauthorizedException("Email ou senha inválidos");
        }

        var token = _jwtService.GenerateToken(usuario.Id, usuario.Email, usuario.Perfil);

        return new LoginResponse(
            AccessToken: token,
            ExpiresIn: 3600,
            Usuario: new UsuarioDto(
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.Perfil,
                DateTime.UtcNow
            )
        );
    }
}