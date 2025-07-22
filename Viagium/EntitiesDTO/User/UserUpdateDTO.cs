using System;
using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO.User
{
    public class UserUpdateDto
    {
        [Required]
        public int UserId { get; set; }
        
        [EmailAddress]
        public required string Email { get; set; }
        
        [MaxLength(100)]
        public required string FirstName { get; set; }
        
        [MaxLength(100)]
        public required string LastName { get; set; }
        
        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}
