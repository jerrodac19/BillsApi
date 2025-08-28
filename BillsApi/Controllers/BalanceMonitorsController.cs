using BillsApi.Dtos;
using BillsApi.Models;
using BillsApi.Repositories.UnitOfWork;
using BillsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BillsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BalanceMonitorsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly BalanceAnalyticsService _analyticsService;

        public BalanceMonitorsController(IUnitOfWork unitOfWork, BalanceAnalyticsService analyticsService)
        {
            _unitOfWork = unitOfWork;
            _analyticsService = analyticsService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BalanceMonitor>>> GetLatestBalanceMonitors()
        {
            var latestDailyBalances = await _unitOfWork.BalanceMonitors.GetLatestDailyBalancesAsync();

            if (latestDailyBalances == null || !latestDailyBalances.Any())
            {
                return NotFound();
            }

            return Ok(latestDailyBalances);
        }

        [HttpPost]
        public async Task<ActionResult<BalanceMonitor>> UpdateMonitor([FromBody] CreateAccountBalanceDto createAccountBalanceDto)
        {
            var accountBalance = new BalanceMonitor
            {
                Amount = createAccountBalanceDto.Amount,
                Updated = createAccountBalanceDto.Updated
            };

            await _unitOfWork.BalanceMonitors.AddAsync(accountBalance);
            await _unitOfWork.SaveAsync();

            return CreatedAtAction(nameof(GetLatestBalanceMonitors), new { id = accountBalance.Id }, accountBalance);
        }

        [HttpGet("analytics")]
        public async Task<ActionResult<BalanceAnalyticsResult>> GetAnalytics()
        {
            var data = await _unitOfWork.BalanceMonitors.GetLatestDailyBalancesAsync();
            var balanceMonitors = data.ToList();

            var normalizedData = _analyticsService.NormalizeLentMoney(
                balanceMonitors,
                3019,
                new DateTime(2024, 11, 15, 7, 0, 0),
                new DateTime(2025, 1, 16, 7, 0, 0)
            );

            if (normalizedData == null || normalizedData.Count() < 3)
            {
                return BadRequest("Not enough data points for analysis.");
            }

            var result = _analyticsService.Analyze(normalizedData);

            return Ok(result);
        }
    }
}