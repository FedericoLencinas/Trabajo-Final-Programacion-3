namespace Cartera_Cripto.Models.DTOs
{
    public class ChangePasswordDto
    {
        public int? id { get; set; } 
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; } 
    }
}
