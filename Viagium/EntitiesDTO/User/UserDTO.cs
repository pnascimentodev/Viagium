using System;

namespace Viagium.EntitiesDTO.User
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string DocumentNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public required string Phone { get; set; }
        public required string Role { get; set; }
        public bool IsActive { get; set; }
        public required string HashPassword { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }
    }
}
