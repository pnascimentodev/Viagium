using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO.Reservation
{
    public class CreateReservationDTO
    {
        //DTO pra Create e Update Reservation
        [Required]
        public int UserId { get; set; }

        // Id do pacote de viagem
        [Required]
        public int TravelPackageId { get; set; }

        // Id do hotel
        [Required]
        public int HotelId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        // Id do quarto do hotel escolhido
        [Required]
        public int RoomTypeId { get; set; }

        // Permite enviar os dados dos viajantes
        public List<TravelerDTO>? Travelers { get; set; }
    }
}
