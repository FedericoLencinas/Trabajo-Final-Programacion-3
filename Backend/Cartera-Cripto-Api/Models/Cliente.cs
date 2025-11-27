using System.ComponentModel.DataAnnotations;

namespace Cartera_Cripto.Models
{
    public class Cliente
    {
        [Key]
        public int id { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        public string password { get; set; }

    }
}
