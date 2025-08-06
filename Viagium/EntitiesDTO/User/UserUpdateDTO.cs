using System;
using System.ComponentModel.DataAnnotations;

namespace Viagium.EntitiesDTO.User
{
    public class UserUpdateDto
    {
        [Required]
        public int UserId { get; set; }
        
        [EmailAddress]
        public string? Email { get; set; }
        
        [MaxLength(100)]
        public string? FirstName { get; set; }
        
        [MaxLength(100)]
        public string? LastName { get; set; }
        
        public DateTime? BirthDate { get; set; }

        public string? Password { get; set; }
        
        [Phone]
        public string? Phone { get; set; }
    }
}
