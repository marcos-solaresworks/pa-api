using FluentValidation;
using ApiCentral.Application.DTOs;

namespace ApiCentral.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ter um formato válido");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter pelo menos 6 caracteres");
    }
}

public class CreateClienteRequestValidator : AbstractValidator<CreateClienteRequest>
{
    public CreateClienteRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ter um formato válido")
            .MaximumLength(150).WithMessage("Email deve ter no máximo 150 caracteres");

        RuleFor(x => x.Telefone)
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Telefone));

        RuleFor(x => x.Empresa)
            .MaximumLength(150).WithMessage("Empresa deve ter no máximo 150 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Empresa));
    }
}

public class UploadLoteRequestValidator : AbstractValidator<UploadLoteRequest>
{
    public UploadLoteRequestValidator()
    {
        RuleFor(x => x.ClienteId)
            .NotEmpty().WithMessage("ClienteId é obrigatório");

        RuleFor(x => x.PerfilProcessamentoId)
            .NotEmpty().WithMessage("PerfilProcessamentoId é obrigatório");

        RuleFor(x => x.NomeArquivo)
            .NotEmpty().WithMessage("Nome do arquivo é obrigatório")
            .MaximumLength(200).WithMessage("Nome do arquivo deve ter no máximo 200 caracteres");

        RuleFor(x => x.ArquivoBase64)
            .NotEmpty().WithMessage("Arquivo é obrigatório")
            .Must(BeValidBase64).WithMessage("Arquivo deve estar em formato Base64 válido");
    }

    private static bool BeValidBase64(string base64String)
    {
        if (string.IsNullOrEmpty(base64String))
            return false;

        try
        {
            Convert.FromBase64String(base64String);
            return true;
        }
        catch
        {
            return false;
        }
    }
}