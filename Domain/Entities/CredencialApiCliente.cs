using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCentral.Domain.Entities;

public class CredencialApiCliente
{
    public int Id { get; set; }
    
    [Required]
    public int ClienteId { get; set; }
    
    [MaxLength(200)]
    public string? ApiKey { get; set; }
    
    [MaxLength(200)]
    public string? SecretKey { get; set; }
    
    public bool Ativo { get; set; } = true;
    
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("ClienteId")]
    public virtual Cliente Cliente { get; set; } = null!;
}