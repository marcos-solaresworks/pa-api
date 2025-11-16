using MediatR;
using ApiCentral.Application.Commands;
using ApiCentral.Application.DTOs;
using ApiCentral.Domain.Entities;
using ApiCentral.Domain.Interfaces;
using ApiCentral.Domain.Exceptions;

namespace ApiCentral.Application.Handlers
{
    public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, ClienteResponseDto>
    {
        private readonly IClienteRepository _clienteRepository;

        public CreateClienteCommandHandler(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<ClienteResponseDto> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
        {
            // Verificar se já existe cliente com mesmo email
            var existingCliente = await _clienteRepository.GetByEmailAsync(request.Email);
            if (existingCliente != null)
            {
                throw new DomainException("Já existe um cliente cadastrado com este email.");
            }

            var cliente = new Cliente
            {
                Nome = request.Nome,
                Email = request.Email,
                Telefone = request.Telefone,
                Empresa = request.Empresa,
                DataCriacao = DateTime.UtcNow,
                Ativo = true
            };

            var createdCliente = await _clienteRepository.AddAsync(cliente);

            return new ClienteResponseDto(
                createdCliente.Id,
                createdCliente.Nome,
                createdCliente.Email,
                createdCliente.Telefone,
                createdCliente.Empresa,
                createdCliente.DataCriacao,
                createdCliente.Ativo
            );
        }
    }
}