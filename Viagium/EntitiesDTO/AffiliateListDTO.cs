using System;
using System.Collections.Generic;

namespace Viagium.EntitiesDTO
{
    public class AffiliateListDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string StateRegistration { get; set; } = string.Empty;
        public string NumberCadastur { get; set; } = string.Empty;
        public bool IsActiveCadastur { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public AddressListDTO? Address { get; set; }
        public List<HotelDTO>? Hotels { get; set; }
    }

    public class AddressListDTO
    {
        public string StreetName { get; set; } = string.Empty;
        public int AddressNumber { get; set; }
        public string Neighborhood { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}

