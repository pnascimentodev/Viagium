using System.ComponentModel.DataAnnotations;

namespace Viagium.Models;

public class Hotel
{
    [Key]
    [Display(Name = "ID do Hotel")]
    public int HotelId { get; set; }
    
    [Required(ErrorMessage = "O nome do hotel é obrigatório.")]
    [Display(Name = "Nome Fantasia do Hotel")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O endereço do hotel é obrigatório.")]
    [Display(Name = "Endereço do Hotel")]
    public string Location { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A descrição do hotel é obrigatória.")]
    [Display(Name = "Descrição do Hotel")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "A URL da imagem do hotel é obrigatória.")]
    [Display(Name = "URL da Imagem do Hotel")]
    public string ImageUrl { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O telefone de contato do hotel é obrigatório.")]
    [Display(Name = "Telefone de Contato do Hotel")]
    public string ContactNumber { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    // Relacionamento com Affiliate
    public int AffiliateId { get; set; }
    public Affiliate? Affiliate { get; set; }
}