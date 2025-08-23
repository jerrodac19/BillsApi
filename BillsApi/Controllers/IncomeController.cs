using BillsApi.Models;
using BillsApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncomeController : ControllerBase
    {
        private readonly BillsApiContext _context;

        public IncomeController(BillsApiContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Income>>> GetIncome([FromQuery] int groupId = 1)
        {
            if (_context.Incomes == null)
            {
                return NotFound();
            }

            var query = _context.Incomes.Include(i => i.User).AsQueryable();

            query = query.Where(i => i.User.GroupId == groupId);

            return await query.ToListAsync();
        }
    }
}
