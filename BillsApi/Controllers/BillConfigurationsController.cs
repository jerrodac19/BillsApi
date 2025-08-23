using BillsApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillConfigurationsController : ControllerBase
    {
        private readonly BillsApiContext _context;

        public BillConfigurationsController(BillsApiContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BillConfiguration>>> GetBillConfigurations()
        {
            var configurations = await _context.BillConfigurations.ToListAsync();

            return Ok(configurations);
        }
    }
}
