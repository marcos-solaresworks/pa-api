using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCentral.Domain.Entities;

public class ProcessamentoLog
{
    public int Id { get; set; }
    
    [Required]
    public int LoteProcessamentoId { get; set; }
    
    public string? Mensagem { get; set; }
    
    [MaxLength(20)]
    public string? TipoLog { get; set; }
    
    public DateTime DataHora { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey("LoteProcessamentoId")]
    public virtual LoteProcessamento LoteProcessamento { get; set; } = null!;
}