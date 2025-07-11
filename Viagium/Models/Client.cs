using System.ComponentModel.DataAnnotations;

namespace Viagium.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }
    }
}
