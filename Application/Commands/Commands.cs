using MediatR;
using ApiCentral.Application.DTOs;

namespace ApiCentral.Application.Commands;

public record LoginCommand(string Email, string Senha) : IRequest<LoginResponse>;

public record CreateClienteCommand(
    string Nome, 
    string Email, 
    string? Telefone, 
    string? Empresa
) : IRequest<ClienteResponseDto>;

public record UpdateClienteCommand(
    int Id,
    string Nome, 
    string Email, 
    string? Telefone, 
    string? Empresa,
    bool Ativo
) : IRequest<ClienteResponseDto>;

public record UploadLoteCommand(
    int ClienteId,
    int PerfilProcessamentoId,
    string ArquivoBase64,
    string NomeArquivo,
    int UsuarioId
) : IRequest<UploadLoteResponse>;

public record AddLogCommand(
    int LoteId,
    string Mensagem,
    string? TipoLog
) : IRequest<Unit>;