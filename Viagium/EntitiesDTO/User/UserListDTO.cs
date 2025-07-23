using System;

namespace Viagium.EntitiesDTO.User
{
    public class UserListDTO
    {
        public int UserId { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string DocumentNumber { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
