﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Viagium.Models
{
    public class Address
    {
        [Key]
        public int AdressId { get; set; }
        [Required(ErrorMessage = "O nome da rua é obrigatório.")]
        [Display(Name = "Nome da Rua")]
        public string StreetName { get; set; } = string.Empty;
        [Required(ErrorMessage = "O número do endereço é obrigatório.")]
        public int AddressNumber { get; set; }
        [Required(ErrorMessage = "O bairro é obrigatório.")]
        [Display(Name = "Bairro")]
        public string Neighborhood { get; set; } = string.Empty;
        [Required(ErrorMessage = "A cidade é obrigatória.")]
        [Display(Name = "Cidade")]
        public string City { get; set; } = string.Empty;
        [Required(ErrorMessage = "O estado é obrigatório.")]
        [Display(Name = "Estado")]
        public string State { get; set; } = string.Empty;
        [Required(ErrorMessage = "O CEP é obrigatório.")]
        [Display(Name = "CEP")]
        [RegularExpression(@"^\d{5}-\d{3}$", ErrorMessage = "O CEP deve estar no formato 00000-000.")]
        public string ZipCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "O país é obrigatório.")]
        [Display(Name = "País")]
        public string Country { get; set; } = string.Empty;
        [Display(Name = "Data de Criação")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [ForeignKey("Affiliate")]
        [Display(Name = "Id do Afiliado")]
        public int? AffiliateId { get; set; }
        public Affiliate? Affiliate { get; set; }
        public int? HotelId { get; set; }
        [Display(Name = "Id do Hotel")]
        public Hotel? Hotel { get; set; }

    }
}
