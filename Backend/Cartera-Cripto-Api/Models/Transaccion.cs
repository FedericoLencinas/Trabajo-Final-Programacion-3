using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Cartera_Cripto.Models
{
    public class Transaccion
    {
        [Key]
        public int id { get; set; }

        [Required]
        [RegularExpression("purchase|sale", ErrorMessage = "La acción debe ser 'purchase' o 'sale'.")]
        public string action { get; set; }

        [Required]
        public string crypto_code { get; set; }

        [Required]
        public double crypto_amount { get; set; }

        public double money { get; set; }

        public DateTime datetime { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [JsonIgnore]
        public Cliente? Cliente { get; set; }
    }
}