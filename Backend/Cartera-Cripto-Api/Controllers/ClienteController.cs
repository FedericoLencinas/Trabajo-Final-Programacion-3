using Cartera_Cripto.Data;
using Cartera_Cripto.Models;
using Cartera_Cripto.Validaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Cartera_Cripto.Models.DTOs;

namespace Cartera_Cripto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly AppDBcontext _context;

        public ClienteController(AppDBcontext context)
        {
            _context = context;
        }

        // GET api/cliente 
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> Get()
        {
            var clientes = await _context.Clientes.ToListAsync();
            clientes.ForEach(c => c.password = null);
            return Ok(clientes);
        }

        // GET api/cliente/{id} 
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> Get(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound();

            cliente.password = null;
            return Ok(cliente);
        }

        // POST api/cliente/register 
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] Cliente cliente)
        {
            cliente.email = cliente.email.Trim();
            cliente.password = cliente.password.Trim();

            if (!Validacion.ValidarEmail(cliente.email))
                return BadRequest(new { message = "El email no es válido." });

            if (string.IsNullOrWhiteSpace(cliente.password))
                return BadRequest(new { message = "La contraseña es obligatoria." });

            if (string.IsNullOrWhiteSpace(cliente.name))
                return BadRequest(new { message = "El nombre es obligatorio." });

            var existe = await _context.Clientes.AnyAsync(c => c.email == cliente.email);
            if (existe)
                return BadRequest(new { message = "El email ya está registrado." });

            cliente.id = 0;
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                cliente.id,
                cliente.email,
                cliente.name
            });
        }

        // POST api/cliente/login 
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] Models.DTOs.ClienteDto loginCliente)
        {
            string emailLimpio = loginCliente.email.Trim();
            string passwordLimpia = loginCliente.password.Trim();

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.email == emailLimpio && c.password == passwordLimpia);

            if (cliente == null)
                return Unauthorized(new { message = "Email o contraseña incorrectos" });

            cliente.password = null;
            return Ok(cliente);
        }

        // PATCH api/cliente/{id} 
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, [FromBody] ClientePatchDto clientePatch)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound(new { message = "Cliente no encontrado." });

            if (!string.IsNullOrWhiteSpace(clientePatch.name))
                cliente.name = clientePatch.name.Trim();

            if (!string.IsNullOrWhiteSpace(clientePatch.email))
            {
                if (cliente.email.ToLower() != clientePatch.email.ToLower())
                {
                    var existe = await _context.Clientes.AnyAsync(c => c.email.ToLower() == clientePatch.email.ToLower() && c.id != id);
                    if (existe)
                        return BadRequest(new { message = "El email ya está registrado por otro usuario." });
                }

                cliente.email = clientePatch.email.Trim();
            }

            await _context.SaveChangesAsync();

            cliente.password = null;
            return Ok(cliente);
        }

        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            if (request.id == null)
                return BadRequest(new { message = "ID de cliente no proporcionado." });

            var cliente = await _context.Clientes.FindAsync(request.id);
            if (cliente == null)
                return NotFound(new { message = "Cliente no encontrado." });

            if (cliente.password != request.CurrentPassword.Trim())
            {
                return Unauthorized(new { message = "La contraseña actual es incorrecta." });
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Trim().Length < 6)
            {
                return BadRequest(new { message = "La nueva contraseña es demasiado corta (mínimo 6 caracteres) o vacía." });
            }

            cliente.password = request.NewPassword.Trim();

            await _context.SaveChangesAsync();

            return Ok(new { message = "Contraseña actualizada con éxito." });
        }

        // DELETE api/cliente/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            var transacciones = _context.Transacciones.Where(t => t.ClienteId == id);
            _context.Transacciones.RemoveRange(transacciones);

            _context.Clientes.Remove(cliente);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}