using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCentral.Domain.Entities;

public class LoteRegistro
{
    public int Id { get; set; }
    
    [Required]
    public int LoteId { get; set; }
    
    [MaxLength(200)]
    public string? Nome { get; set; }
    
    [MaxLength(300)]
    public string? Endereco { get; set; }
    
    [MaxLength(100)]
    public string? Bairro { get; set; }
    
    [MaxLength(100)]
    public string? Cidade { get; set; }
    
    [MaxLength(2)]
    public string? Uf { get; set; }
    
    [MaxLength(20)]
    public string? Cep { get; set; }
    
    // Navigation properties
    [ForeignKey("LoteId")]
    public virtual LoteProcessamento Lote { get; set; } = null!;
}