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
                .OrderByDescending(t => t.datetime)
                .Select(t => new
                {
                    t.id,
                    t.action,
                    t.crypto_code,
                    t.ClienteId,
                    cliente_nombre = t.Cliente != null ? t.Cliente.name : "",
                    crypto_amount = (double)t.crypto_amount,
                    money = (double)t.money,
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

        [HttpGet("historial/{clienteId}")]
        public async Task<ActionResult> GetHistorialByClienteId(int clienteId)
        {
            if (clienteId <= 0)
            {
                return BadRequest(new { message = "ID de cliente inválido." });
            }

            try
            {
                var transacciones = await _context.Transacciones
                    .Where(t => t.ClienteId == clienteId)
                    .OrderByDescending(t => t.datetime)
                    .Select(t => new
                    {
                        t.id,
                        t.action,
                        t.crypto_code,
                        t.ClienteId,
                        crypto_amount = (double)t.crypto_amount,
                        money = (double)t.money,
                        t.datetime
                    })
                    .ToListAsync();

                if (!transacciones.Any())
                {
                    return NotFound(new { message = $"No se encontraron transacciones para el cliente ID {clienteId}." });
                }

                return Ok(transacciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error interno en la base de datos al buscar historial.",
                    detail = ex.Message
                });
            }
        }

        [HttpGet("balance/{clienteId}")]
        public async Task<ActionResult> GetBalance(int clienteId)
        {
            if (clienteId <= 0)
            {
                return BadRequest(new { message = "ID de cliente inválido." });
            }

            try
            {
                var saldos = await _context.Transacciones
                    .Where(t => t.ClienteId == clienteId)
                    .GroupBy(t => t.crypto_code) 
                    .Select(g => new
                    {
                        crypto_code = g.Key,
                        balance = g.Sum(t => (double)(t.action.ToLower() == "purchase" ? t.crypto_amount : -t.crypto_amount))
                    })
                    .Where(s => s.balance > 0) 
                    .OrderByDescending(s => s.balance)
                    .ToListAsync();

                if (!saldos.Any())
                {
                    return NotFound(new { message = "No se encontraron saldos para este cliente." });
                }

                return Ok(saldos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error interno al calcular el balance.",
                    detail = ex.Message
                });
            }
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

            double precioActual;
            try
            {
                precioActual = await ObtenerPrecioActualARS(transaccion.crypto_code);
            }
            catch
            {
                return BadRequest("Error obteniendo precio desde CriptoYa.");
            }

            transaccion.money = transaccion.crypto_amount * precioActual;
            transaccion.datetime = DateTime.Now;

            if (transaccion.action.ToLower() == "sale")
            {
                var movimientos = await _context.Transacciones
                    .Where(t => t.ClienteId == transaccion.ClienteId &&
                                 t.crypto_code.ToLower() == transaccion.crypto_code.ToLower())
                    .ToListAsync();

                double saldo = movimientos.Sum(t =>
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

            var clienteExistente = await _context.Clientes.FindAsync(transaccion.ClienteId);
            if (clienteExistente == null)
                return NotFound("Cliente no encontrado.");

            var transaccionExistente = await _context.Transacciones.FindAsync(id);
            if (transaccionExistente == null)
                return NotFound("La transacción no existe.");

            if (transaccion.action.ToLower() != "purchase" &&
                transaccion.action.ToLower() != "sale")
                return BadRequest("La acción debe ser 'purchase' o 'sale'.");

            double precioActual;
            try
            {
                precioActual = await ObtenerPrecioActualARS(transaccion.crypto_code);
            }
            catch
            {
                return BadRequest("Error obteniendo precio desde CriptoYa.");
            }

            transaccionExistente.datetime = DateTime.Now;
            transaccionExistente.money = transaccion.crypto_amount * precioActual;

            if (transaccion.action.ToLower() == "sale")
            {
                var saldoTotalMoneda = await _context.Transacciones
                    .Where(t => t.ClienteId == transaccion.ClienteId &&
                                t.crypto_code.ToLower() == transaccion.crypto_code.ToLower())
                    .SumAsync(t => (double)(t.action.ToLower() == "purchase" ? t.crypto_amount : -t.crypto_amount));

                double valorExistente = (transaccionExistente.action.ToLower() == "purchase")
                                        ? transaccionExistente.crypto_amount
                                        : -transaccionExistente.crypto_amount;

                double saldoDisponibleNeto = saldoTotalMoneda - valorExistente;

                if (transaccion.crypto_amount > saldoDisponibleNeto)
                {
                    return BadRequest($"Saldo insuficiente para realizar la venta de {transaccion.crypto_amount} {transaccion.crypto_code}. Saldo disponible neto: {saldoDisponibleNeto.ToString("N8")}");
                }
            }

            transaccionExistente.crypto_code = transaccion.crypto_code;
            transaccionExistente.action = transaccion.action;
            transaccionExistente.crypto_amount = transaccion.crypto_amount;

            await _context.SaveChangesAsync();

            return Ok(transaccionExistente);
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

        private async Task<double> ObtenerPrecioActualARS(string cryptoCode)
        {
            using var httpClient = new HttpClient();

            string url = $"https://criptoya.com/api/{cryptoCode.ToLower()}/ars";

            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(jsonString);

            if (jsonDoc.RootElement.TryGetProperty("satoshitango", out JsonElement sato))
            {
                if (sato.TryGetProperty("ask", out JsonElement ask) &&
                    ask.TryGetDecimal(out decimal precio))
                {
                    return (double)precio;
                }
            }

            throw new Exception($"No se pudo obtener el precio 'ask' de SatoshiTango para {cryptoCode}.");
        }
    }
}