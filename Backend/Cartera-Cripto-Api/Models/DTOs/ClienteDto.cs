using System.ComponentModel.DataAnnotations;

namespace Cartera_Cripto.Models.DTOs
{
    public class ClienteDto
    {
        [Required]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)] 
        public string password { get; set; }
    }
}