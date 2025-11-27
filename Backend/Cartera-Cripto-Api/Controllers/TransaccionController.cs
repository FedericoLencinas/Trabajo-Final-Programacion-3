using Cartera_Cripto.Models;
using Cartera_Cripto.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Cartera_Cripto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransaccionController : ControllerBase
    {
        private readonly AppDBcontext _context;

        public TransaccionController(AppDBcontext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var transacciones = await _context.Transacciones
                .Include(t => t.Cliente)
                .Select(t => new
                {
                    t.id,
                    t.action,
                    t.crypto_code,
                    t.ClienteId,
                    cliente_nombre = t.Cliente != null ? t.Cliente.name : "",
                    t.crypto_amount,
                    t.money,
                    t.datetime
                })
                .ToListAsync();

            return Ok(transacciones);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Get(int id)
        {
            var transaccion = await _context.Transacciones
                .Include(t => t.Cliente)
                .FirstOrDefaultAsync(t => t.id == id);

            if (transaccion == null)
                return NotFound();

            return Ok(transaccion);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Transaccion transaccion)
        {
            transaccion.id = 0;

            var cliente = await _context.Clientes.FindAsync(transaccion.ClienteId);
            if (cliente == null)
                return BadRequest("Cliente no existe.");

            if (transaccion.action.ToLower() != "purchase" &&
                transaccion.action.ToLower() != "sale")
                return BadRequest("La acción debe ser 'purchase' o 'sale'.");

            decimal precioActual;
            try
            {
                precioActual = await ObtenerPrecioActualARS(transaccion.crypto_code);
            }
            catch
            {
                return BadRequest("Error obteniendo precio desde CriptoYa.");
            }

            transaccion.money = (float)((decimal)transaccion.crypto_amount * precioActual);
            transaccion.datetime = DateTime.Now;

            if (transaccion.action.ToLower() == "sale")
            {
                var movimientos = await _context.Transacciones
                    .Where(t => t.ClienteId == transaccion.ClienteId &&
                                t.crypto_code.ToLower() == transaccion.crypto_code.ToLower())
                    .ToListAsync();

                float saldo = movimientos.Sum(t =>
                    t.action.ToLower() == "purchase" ? t.crypto_amount : -t.crypto_amount);

                if (transaccion.crypto_amount > saldo)
                    return BadRequest("Saldo insuficiente para realizar la venta.");
            }

            _context.Transacciones.Add(transaccion);
            await _context.SaveChangesAsync();

            return Ok(transaccion);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Transaccion transaccion)
        {
            if (id != transaccion.id)
                return BadRequest("El ID no coincide.");

            var existente = await _context.Transacciones.FindAsync(id);
            if (existente == null)
                return NotFound("La transacción no existe.");

            if (transaccion.action.ToLower() != "purchase" &&
                transaccion.action.ToLower() != "sale")
                return BadRequest("La acción debe ser 'purchase' o 'sale'.");

            transaccion.ClienteId = existente.ClienteId;

            decimal precioActual;
            try
            {
                precioActual = await ObtenerPrecioActualARS(transaccion.crypto_code);
            }
            catch
            {
                return BadRequest("Error obteniendo precio desde CriptoYa.");
            }

            if (transaccion.action.ToLower() == "sale")
            {
                var movimientos = await _context.Transacciones
                    .Where(t => t.ClienteId == existente.ClienteId &&
                                t.crypto_code.ToLower() == transaccion.crypto_code.ToLower() &&
                                t.id != id)
                    .ToListAsync();

                float saldo = movimientos.Sum(t =>
                    t.action.ToLower() == "purchase" ? t.crypto_amount : -t.crypto_amount);

                if (transaccion.crypto_amount > saldo)
                    return BadRequest("Saldo insuficiente para realizar la venta.");
            }

            existente.crypto_code = transaccion.crypto_code;
            existente.action = transaccion.action;
            existente.crypto_amount = transaccion.crypto_amount;
            existente.money = (float)((decimal)transaccion.crypto_amount * precioActual);
            existente.datetime = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(existente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var transaccion = await _context.Transacciones.FindAsync(id);

            if (transaccion == null)
                return NotFound();

            _context.Transacciones.Remove(transaccion);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<decimal> ObtenerPrecioActualARS(string cryptoCode)
        {
            using var httpClient = new HttpClient();

            string url = $"https://criptoya.com/api/{cryptoCode.ToLower()}/ars";
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(jsonString);

            if (jsonDoc.RootElement.TryGetProperty("satoshitango", out JsonElement sato) &&
                sato.TryGetProperty("ask", out JsonElement ask) &&
                ask.TryGetDecimal(out decimal precio))
            {
                return precio;
            }

            throw new Exception("No se pudo obtener precio desde CriptoYa.");
        }
    }
}
