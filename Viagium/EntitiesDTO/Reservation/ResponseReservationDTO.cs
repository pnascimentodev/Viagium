using System.ComponentModel.DataAnnotations;
using Viagium.EntitiesDTO.User;

namespace Viagium.EntitiesDTO.Reservation
{
    public class ResponseReservationDTO
    {
        public int ReservationId { get; set; }
        
        // Dados do usuário
        public UserDTO? User { get; set; }

        // Dados dos viajantes adicionais (opcional)
        public List<TravelerDTO>? Travelers { get; set; }

        // Dados do pacote
        public TravelPackageDTO.TravelPackageDTO? TravelPackage { get; set; }

        // Dados do hotel
        public HotelDTO? Hotel { get; set; }

        // Dados do quarto do hotel
        public RoomTypeDTO? RoomType { get; set; }

        public bool IsActive { get; internal set; }
        
        public string Status { get; set; }
        
        [Display(Name = "Data de Criação")]
        public DateTime CreatedAt { get; set; }
        
        
        [DataType(DataType.Date)]        
        public DateTime StartDate { get; set; }
        
        [DataType(DataType.Date)]        
        public DateTime EndDate { get; set; }
    }
}
