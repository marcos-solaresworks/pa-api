using Microsoft.EntityFrameworkCore;
using ApiCentral.Domain.Entities;

namespace ApiCentral.Infrastructure.Data;

public class ApiCentralDbContext : DbContext
{
    public ApiCentralDbContext(DbContextOptions<ApiCentralDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<PerfilProcessamento> PerfisProcessamento { get; set; }
    public DbSet<LoteProcessamento> LotesProcessamento { get; set; }
    public DbSet<LoteRegistro> LoteRegistros { get; set; }
    public DbSet<ProcessamentoLog> ProcessamentoLogs { get; set; }
    public DbSet<CredencialApiCliente> CredenciaisApiClientes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração de tabelas
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nome).HasColumnName("nome");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.SenhaHash).HasColumnName("senha_hash");
            entity.Property(e => e.Perfil).HasColumnName("perfil");
            entity.Property(e => e.UltimoLogin).HasColumnName("ultimo_login");
            entity.Property(e => e.DataCriacao).HasColumnName("data_criacao");
            
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("clientes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nome).HasColumnName("nome");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.Telefone).HasColumnName("telefone");
            entity.Property(e => e.Empresa).HasColumnName("empresa");
            entity.Property(e => e.DataCriacao).HasColumnName("data_criacao");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<PerfilProcessamento>(entity =>
        {
            entity.ToTable("perfis_processamento");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Nome).HasColumnName("nome");
            entity.Property(e => e.Descricao).HasColumnName("descricao");
            entity.Property(e => e.TipoArquivo).HasColumnName("tipo_arquivo");
            entity.Property(e => e.Delimitador).HasColumnName("delimitador");
            entity.Property(e => e.TemplatePcl).HasColumnName("template_pcl");
            entity.Property(e => e.DataCriacao).HasColumnName("data_criacao");
            
            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.PerfisProcessamento)
                .HasForeignKey(e => e.ClienteId);
        });

        modelBuilder.Entity<LoteProcessamento>(entity =>
        {
            entity.ToTable("lotes_processamento");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.PerfilProcessamentoId).HasColumnName("perfil_processamento_id");
            entity.Property(e => e.NomeArquivo).HasColumnName("nome_arquivo");
            entity.Property(e => e.CaminhoS3).HasColumnName("caminho_s3");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.DataCriacao).HasColumnName("data_criacao");
            entity.Property(e => e.DataProcessamento).HasColumnName("data_processamento");

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.LotesProcessamento)
                .HasForeignKey(e => e.ClienteId);

            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.LotesProcessamento)
                .HasForeignKey(e => e.UsuarioId);

            entity.HasOne(e => e.PerfilProcessamento)
                .WithMany(p => p.LotesProcessamento)
                .HasForeignKey(e => e.PerfilProcessamentoId);
        });

        modelBuilder.Entity<LoteRegistro>(entity =>
        {
            entity.ToTable("lote_registros");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LoteId).HasColumnName("lote_id");
            entity.Property(e => e.Nome).HasColumnName("nome");
            entity.Property(e => e.Endereco).HasColumnName("endereco");
            entity.Property(e => e.Bairro).HasColumnName("bairro");
            entity.Property(e => e.Cidade).HasColumnName("cidade");
            entity.Property(e => e.Uf).HasColumnName("uf");
            entity.Property(e => e.Cep).HasColumnName("cep");

            entity.HasOne(e => e.Lote)
                .WithMany(l => l.Registros)
                .HasForeignKey(e => e.LoteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProcessamentoLog>(entity =>
        {
            entity.ToTable("processamento_logs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.LoteProcessamentoId).HasColumnName("lote_processamento_id");
            entity.Property(e => e.Mensagem).HasColumnName("mensagem");
            entity.Property(e => e.TipoLog).HasColumnName("tipo_log");
            entity.Property(e => e.DataHora).HasColumnName("data_hora");

            entity.HasOne(e => e.LoteProcessamento)
                .WithMany(l => l.Logs)
                .HasForeignKey(e => e.LoteProcessamentoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CredencialApiCliente>(entity =>
        {
            entity.ToTable("credenciais_api_clientes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.ApiKey).HasColumnName("api_key");
            entity.Property(e => e.SecretKey).HasColumnName("secret_key");
            entity.Property(e => e.Ativo).HasColumnName("ativo");
            entity.Property(e => e.CriadoEm).HasColumnName("criado_em");

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.CredenciaisApi)
                .HasForeignKey(e => e.ClienteId);
        });
    }
}