using Cartera_Cripto.Data;
using Cartera_Cripto.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cartera_Cripto.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MonedaController : ControllerBase
    {
        private readonly AppDBcontext _context;

        public MonedaController(AppDBcontext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            var monedas = await _context.Monedas.ToListAsync();
            return Ok(monedas);
        }
    }
}
