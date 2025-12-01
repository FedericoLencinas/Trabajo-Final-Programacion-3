using Cartera_Cripto.Data;
using Cartera_Cripto.Models;
using Cartera_Cripto.Validaciones;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


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
        public async Task<IActionResult> Login([FromBody] Cliente loginCliente)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.email == loginCliente.email && c.password == loginCliente.password);

            if (cliente == null)
                return Unauthorized(new { message = "Email o contraseña incorrectos" });

            // Autenticación exitosa
            cliente.password = null;
            return Ok(cliente);
        }

        // PATCH api/cliente/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, [FromBody] Cliente clientePatch)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            if (!string.IsNullOrWhiteSpace(clientePatch.name))
                cliente.name = clientePatch.name;

            if (!string.IsNullOrWhiteSpace(clientePatch.email))
            {
                if (!Validacion.ValidarEmail(clientePatch.email))
                    return BadRequest("Email inválido.");

                cliente.email = clientePatch.email;
            }

            if (!string.IsNullOrWhiteSpace(clientePatch.password))
                cliente.password = clientePatch.password;

            await _context.SaveChangesAsync();

            cliente.password = null;
            return Ok(cliente);
        }

        // DELETE api/cliente/{id} 
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound("Cliente no encontrado.");

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}