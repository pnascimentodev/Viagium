using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO.Reservation
{
    public class CreateReservationDTO
    {
        //DTO pra Create e Update Reservation
        [Required]
        public int UserId { get; set; }

        [Required]
        public int TravelPackageId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        // Permite enviar os dados dos viajantes
        public List<TravelerDTO>? Travelers { get; set; }

        // Permite enviar os dados dos quartos reservados
        public List<ReservationRoomDTO>? ReservationRooms { get; set; }

        // Permite enviar os dados do pagamento
        public PaymentDTO? Payment { get; set; }
    }
}
