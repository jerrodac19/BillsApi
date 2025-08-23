using BillsApi.Dtos;
using BillsApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountBalancesController : ControllerBase
    {
        private readonly BillsApiContext _context;

        public AccountBalancesController(BillsApiContext context)
        {
            _context = context;
        }

        [HttpGet("latest")]
        public async Task<ActionResult<IEnumerable<AccountBalance>>> GetLatestAccountBalance()
        {
            if (_context.AccountBalances == null)
            {
                return NotFound();
            }

            // Find the latest entry by ordering in descending order by Date
            var latestBalance = await _context.AccountBalances
                .OrderByDescending(ab => ab.Id)
                .FirstOrDefaultAsync();

            if (latestBalance == null)
            {
                return NotFound();
            }

            return Ok(latestBalance);
        }

        [HttpPost]
        public async Task<ActionResult<AccountBalance>> UpdateBalance([FromBody] CreateAccountBalanceDto createAccountBalanceDto)
        {
            if (_context.AccountBalances == null)
            {
                return NotFound();
            }

            var accountBalance = new AccountBalance
            {
                Amount = createAccountBalanceDto.Amount,
                Updated = createAccountBalanceDto.Updated,
                GroupId = createAccountBalanceDto.GroupId
            };

            _context.AccountBalances.Add(accountBalance);
            await _context.SaveChangesAsync();

            // Return a 201 Created status code with the newly created AccountBalance
            return CreatedAtAction(nameof(GetLatestAccountBalance), new { id = accountBalance.Id }, accountBalance);
        }

        /// <summary>
        /// Gets the last N balances from the database.
        /// </summary>
        /// <param name="n">The number of balances to return (e.g., 10). Defaults to 10 if not provided.</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountBalance>>> GetLastNBalances([FromQuery] int n = 1)
        {
            if (n <= 0)
            {
                return BadRequest("The number of transactions must be a positive integer.");
            }

            // Order by date descending and take the top N records
            var balances = await _context.AccountBalances
                .OrderByDescending(ab => ab.Id)
                .Take(n)
                .ToListAsync();

            return Ok(balances);
        }
    }
}
