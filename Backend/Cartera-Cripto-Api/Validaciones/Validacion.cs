using Cartera_Cripto.Models;
using System.Text.RegularExpressions; 

namespace Cartera_Cripto.Validaciones
{
    internal abstract class Validacion
    {
        public static bool ValidarEmail(string email)
        {
            
            string MustBeEmail = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
 
            Regex regex = new Regex(MustBeEmail);

            return regex.IsMatch(email);

        }
    }
}
