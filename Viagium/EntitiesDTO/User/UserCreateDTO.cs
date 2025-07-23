using System;
using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO.User
{
    public class UserCreateDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MaxLength(100)]
        public required string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public required string LastName { get; set; }

        [Required]
        public required string DocumentNumber { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public required string Password { get; set; }

    }
}
