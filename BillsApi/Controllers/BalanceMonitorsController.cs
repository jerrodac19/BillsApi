using BillsApi.Dtos;
using BillsApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BalanceMonitorsController : ControllerBase
    {
        private readonly BillsApiContext _context;
        private readonly BalanceAnalyticsService _analyticsService;

        public BalanceMonitorsController(BillsApiContext context, BalanceAnalyticsService analyticsService)
        {
            _context = context;
            _analyticsService = analyticsService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BalanceMonitor>>> GetLatestBalanceMonitors()
        {
            if (_context.BalanceMonitors == null)
            {
                return NotFound();
            }

            var latestDailyBalances = await _context.BalanceMonitors
                .Where(b => b.Updated.HasValue) // Only consider records with a value
                .GroupBy(b => b.Updated!.Value.Date) // Now it's safe to access .Value.Date
                .Select(g => g.OrderByDescending(b => b.Updated).FirstOrDefault())
                .ToListAsync();

            if (latestDailyBalances == null)
            {
                return NotFound();
            }

            return Ok(latestDailyBalances);
        }

        [HttpPost]
        public async Task<ActionResult<BalanceMonitor>> UpdateMonitor([FromBody] CreateAccountBalanceDto createAccountBalanceDto)
        {
            if (_context.AccountBalances == null)
            {
                return NotFound();
            }

            var accountBalance = new BalanceMonitor
            {
                Amount = createAccountBalanceDto.Amount,
                Updated = createAccountBalanceDto.Updated
            };

            _context.BalanceMonitors.Add(accountBalance);
            await _context.SaveChangesAsync();

            // Return a 201 Created status code with the newly created BalanceMonitor
            return CreatedAtAction(nameof(GetLatestBalanceMonitors), new { id = accountBalance.Id }, accountBalance);
        }

        [HttpGet("analytics")]
        public async Task<ActionResult<BalanceAnalyticsResult>> GetAnalytics()
        {
            // Get all balance data from the database
            var data = await _context.BalanceMonitors
                .Where(b => b.Updated.HasValue) // Only consider records with a value
                .GroupBy(b => b.Updated!.Value.Date) // Now it's safe to access .Value.Date
                .Select(g => g.OrderByDescending(b => b.Updated).FirstOrDefault())
                .ToListAsync();

            var nonNullableData = data as List<BalanceMonitor>;
            //ignore dip in balances from when money was borrowed and payed back
            NormalizeBorrowedMoney(nonNullableData, 3019, new DateTime(2024,11,15), new DateTime(2025,1,16));

            if (nonNullableData == null || nonNullableData.Count < 3)
            {
                return BadRequest("Not enough data points for analysis.");
            }

            // Call the C# service to perform the calculations
            var result = _analyticsService.Analyze(nonNullableData);

            return Ok(result);
        }

        private void NormalizeBorrowedMoney(List<BalanceMonitor> balances, decimal borrowedAmount, DateTime startDate, DateTime endDate)
        {
            for (int i = 0; i < balances.Count; i++)
            {
                var b = balances[i];
                if (b.Updated >= startDate && b.Updated <= endDate)
                {
                    b.Amount += borrowedAmount;
                }
            }
        }
    }
}
