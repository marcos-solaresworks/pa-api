using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCentral.Domain.Entities;

public class PerfilProcessamento
{
    public int Id { get; set; }
    
    [Required]
    public int ClienteId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;
    
    public string? Descricao { get; set; }
    
    [MaxLength(20)]
    public string? TipoArquivo { get; set; }
    
    [MaxLength(5)]
    public string? Delimitador { get; set; }
    
    [MaxLength(200)]
    public string? TemplatePcl { get; set; }
    
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("ClienteId")]
    public virtual Cliente Cliente { get; set; } = null!;
    
    public virtual ICollection<LoteProcessamento> LotesProcessamento { get; set; } = new List<LoteProcessamento>();
}