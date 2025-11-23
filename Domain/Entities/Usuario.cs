using System.ComponentModel.DataAnnotations;

namespace ApiCentral.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string SenhaHash { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Perfil { get; set; } = string.Empty;
    
    public DateTime UltimoLogin { get; set; }
    
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<LoteProcessamento> LotesProcessamento { get; set; } = new List<LoteProcessamento>();
}