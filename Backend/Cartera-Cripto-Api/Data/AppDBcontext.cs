using Cartera_Cripto.Models;
using Microsoft.EntityFrameworkCore;

namespace Cartera_Cripto.Data
{
    public class AppDBcontext : DbContext
    {
        public AppDBcontext(DbContextOptions<AppDBcontext> options) : base(options)
        {
        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Transaccion> Transacciones { get; set; }
        public DbSet<Moneda> Monedas { get; set; }
    }
}
