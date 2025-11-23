using System.ComponentModel.DataAnnotations;

namespace ApiCentral.Domain.Entities;

public class Cliente
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? Telefone { get; set; }
    
    [MaxLength(150)]
    public string? Empresa { get; set; }
    
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    
    public bool Ativo { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<LoteProcessamento> LotesProcessamento { get; set; } = new List<LoteProcessamento>();
    public virtual ICollection<CredencialApiCliente> CredenciaisApi { get; set; } = new List<CredencialApiCliente>();
    public virtual ICollection<PerfilProcessamento> PerfisProcessamento { get; set; } = new List<PerfilProcessamento>();
}