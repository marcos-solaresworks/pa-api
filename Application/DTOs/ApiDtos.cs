namespace ApiCentral.Application.DTOs;

public record LoginRequest(string Email, string Senha);

public record LoginResponse(
    string AccessToken,
    int ExpiresIn,
    UsuarioDto Usuario
);

public record UsuarioDto(
    int Id,
    string Nome,
    string Email,
    string Perfil,
    DateTime UltimoLogin
);

public record ClienteDto(
    int Id,
    string Nome,
    string Email,
    string? Telefone,
    string? Empresa,
    DateTime DataCriacao,
    bool Ativo
);

public record ClienteResponseDto(
    int Id,
    string Nome,
    string Email,
    string? Telefone,
    string? Empresa,
    DateTime DataCriacao,
    bool Ativo
);

public record CreateClienteRequest(
    string Nome, 
    string Email, 
    string? Telefone, 
    string? Empresa
);

public record UpdateClienteRequest(
    string Nome, 
    string Email, 
    string? Telefone, 
    string? Empresa,
    bool Ativo
);

public record PerfilProcessamentoDto(
    int Id,
    string Nome,
    string? Descricao,
    int ClienteId,
    string? TipoArquivo,
    string? Delimitador,
    string? TemplatePcl
);

public record CreatePerfilProcessamentoRequest(
    string Nome,
    string? Descricao,
    int ClienteId,
    string? TipoArquivo,
    string? Delimitador,
    string? TemplatePcl
);

public record UploadLoteRequest(
    int ClienteId,
    int PerfilProcessamentoId,
    string ArquivoBase64,
    string NomeArquivo
);

public record UploadLoteResponse(
    int LoteId,
    string Status,
    string Mensagem,
    DateTime DataCriacao
);

public record LoteProcessamentoDto(
    int Id,
    string Cliente,
    string NomeArquivo,
    string Status,
    int RegistrosTotal,
    int RegistrosProcessados,
    DateTime DataCriacao,
    string? UrlArquivoProcessado,
    List<ProcessamentoLogDto> Logs
);

public record ProcessamentoLogDto(
    string Mensagem,
    string? TipoLog,
    DateTime DataHora
);

public record DashboardResumoDto(
    int TotalClientes,
    int ClientesAtivos,
    int TotalLotes,
    int LotesProcessando,
    int LotesConcluidos,
    int LotesComErro,
    int TotalProcessamentos,
    int ProcessamentosHoje,
    int ProcessamentosSucesso,
    int ProcessamentosErro,
    object EstatisticasUltimos30Dias
);