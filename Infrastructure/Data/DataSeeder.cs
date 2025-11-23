using ApiCentral.Domain.Entities;
using ApiCentral.Domain.Interfaces;
using ApiCentral.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace ApiCentral.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApiCentralDbContext context, IPasswordService passwordService)
    {
        // Verificar se já existem usuários (banco recém criado não deve ter)
        if (await context.Usuarios.AnyAsync())
        {
            return; // Já tem dados, não precisa fazer seed
        }

        await SeedClientes(context);
        await SeedUsuarios(context, passwordService);
        await SeedPerfisProcessamento(context);
        
        await context.SaveChangesAsync();
    }

    private static async Task SeedUsuarios(ApiCentralDbContext context, IPasswordService passwordService)
    {
        var usuarios = new List<Usuario>
        {
            // Persona 1: Ana Ribeiro - Operadora
            new Usuario
            {
                Nome = "Ana Ribeiro",
                Email = "ana.ribeiro@graficaltda.com",
                SenhaHash = passwordService.HashPassword("Operadora@123"),
                Perfil = "Operador",
                DataCriacao = DateTime.UtcNow.AddDays(-30),
                UltimoLogin = DateTime.UtcNow.AddHours(-2)
            },

            // Persona 2: Carlos Mendes - Impressão
            new Usuario
            {
                Nome = "Carlos Mendes",
                Email = "carlos.mendes@graficaltda.com",
                SenhaHash = passwordService.HashPassword("Impressao@123"),
                Perfil = "Impressao",
                DataCriacao = DateTime.UtcNow.AddDays(-25),
                UltimoLogin = DateTime.UtcNow.AddHours(-1)
            },

            // Persona 3: Marcos Oliveira - Gestor
            new Usuario
            {
                Nome = "Marcos Oliveira",
                Email = "marcos.oliveira@graficaltda.com",
                SenhaHash = passwordService.HashPassword("Gestor@123"),
                Perfil = "Gestor",
                DataCriacao = DateTime.UtcNow.AddDays(-20),
                UltimoLogin = DateTime.UtcNow.AddMinutes(-30)
            },

            // Usuários adicionais para testes
            new Usuario
            {
                Nome = "Sistema ERP Corporativo",
                Email = "erp@clientecorporativo.com",
                SenhaHash = passwordService.HashPassword("ERP@System123"),
                Perfil = "Cliente",
                DataCriacao = DateTime.UtcNow.AddDays(-22),
                UltimoLogin = DateTime.UtcNow.AddHours(-3)
            },

            new Usuario
            {
                Nome = "Administrador Sistema",
                Email = "admin@graficaltda.com",
                SenhaHash = passwordService.HashPassword("Admin@123"),
                Perfil = "Administrador",
                DataCriacao = DateTime.UtcNow.AddDays(-35),
                UltimoLogin = DateTime.UtcNow.AddHours(-4)
            },

            // Usuários para testes diversos
            new Usuario
            {
                Nome = "Operador Teste",
                Email = "operador.teste@graficaltda.com",
                SenhaHash = passwordService.HashPassword("Teste@123"),
                Perfil = "Operador",
                DataCriacao = DateTime.UtcNow.AddDays(-10),
                UltimoLogin = DateTime.UtcNow.AddDays(-1)
            },

            new Usuario
            {
                Nome = "Cliente Demo",
                Email = "cliente.demo@exemplo.com",
                SenhaHash = passwordService.HashPassword("Demo@123"),
                Perfil = "Cliente",
                DataCriacao = DateTime.UtcNow.AddDays(-5),
                UltimoLogin = DateTime.UtcNow.AddHours(-6)
            }
        };

        await context.Usuarios.AddRangeAsync(usuarios);
    }

    private static async Task SeedClientes(ApiCentralDbContext context)
    {
        var clientes = new List<Cliente>
        {
            new Cliente
            {
                Nome = "Empresa ABC Ltda",
                Email = "contato@empresaabc.com",
                Telefone = "(11) 98765-4321",
                Empresa = "Empresa ABC Ltda",
                Ativo = true,
                DataCriacao = DateTime.UtcNow.AddDays(-30)
            },
            new Cliente
            {
                Nome = "Sistema ERP",
                Email = "api@corporacaoxyz.com",
                Telefone = "(11) 91234-5678",
                Empresa = "Corporação XYZ S.A.",
                Ativo = true,
                DataCriacao = DateTime.UtcNow.AddDays(-20)
            },
            new Cliente
            {
                Nome = "Cliente Demo",
                Email = "sistemas@industria123.com",
                Telefone = "(11) 95555-0000",
                Empresa = "Indústria 123 ME",
                Ativo = true,
                DataCriacao = DateTime.UtcNow.AddDays(-15)
            }
        };

        await context.Clientes.AddRangeAsync(clientes);
    }

    private static async Task SeedPerfisProcessamento(ApiCentralDbContext context)
    {
        var perfis = new List<PerfilProcessamento>
        {
            new PerfilProcessamento
            {
                ClienteId = 1, // Empresa ABC Ltda
                Nome = "Padrão - Impressão Offset",
                Descricao = "Perfil padrão para impressão offset com configurações otimizadas",
                TipoArquivo = "CSV",
                Delimitador = ",",
                TemplatePcl = "template_offset.pcl",
                DataCriacao = DateTime.UtcNow.AddDays(-25)
            },
            new PerfilProcessamento
            {
                ClienteId = 2, // Sistema ERP
                Nome = "Rápido - Impressão Digital",
                Descricao = "Perfil para impressão digital com processamento rápido",
                TipoArquivo = "TXT",
                Delimitador = ";",
                TemplatePcl = "template_digital.pcl",
                DataCriacao = DateTime.UtcNow.AddDays(-20)
            },
            new PerfilProcessamento
            {
                ClienteId = 1, // Empresa ABC Ltda
                Nome = "Premium - Alta Qualidade",
                Descricao = "Perfil premium para trabalhos de alta qualidade",
                TipoArquivo = "CSV",
                Delimitador = ",",
                TemplatePcl = "template_premium.pcl",
                DataCriacao = DateTime.UtcNow.AddDays(-15)
            }
        };

        await context.PerfisProcessamento.AddRangeAsync(perfis);
    }
}