using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCentral.Domain.Entities;

public class LoteProcessamento
{
    public int Id { get; set; }
    
    [Required]
    public int ClienteId { get; set; }
    
    [Required]
    public int UsuarioId { get; set; }
    
    [Required]
    public int PerfilProcessamentoId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string NomeArquivo { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(300)]
    public string CaminhoS3 { get; set; } = string.Empty;
    
    [MaxLength(300)]
    public string? CaminhoProcessadoS3 { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
    
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    
    public DateTime? DataProcessamento { get; set; }
    
    // Navigation properties
    [ForeignKey("ClienteId")]
    public virtual Cliente Cliente { get; set; } = null!;
    
    [ForeignKey("UsuarioId")]
    public virtual Usuario Usuario { get; set; } = null!;
    
    [ForeignKey("PerfilProcessamentoId")]
    public virtual PerfilProcessamento PerfilProcessamento { get; set; } = null!;
    
    public virtual ICollection<LoteRegistro> Registros { get; set; } = new List<LoteRegistro>();
    public virtual ICollection<ProcessamentoLog> Logs { get; set; } = new List<ProcessamentoLog>();
}