using BillsApi.Dtos;
using BillsApi.Models;
using BillsApi.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace BillsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<DashboardDto>> GetDashboardData()
        {
            // Fetch all the data sequentially to avoid DbContext threading issues
            var latestBalance = await _unitOfWork.AccountBalances.GetLatestBalanceAsync();
            var bills = await _unitOfWork.Bills.GetBillsWithConfigurationAsync(null);
            var expectedIncome = await _unitOfWork.Incomes.GetIncomesByGroupIdAsync(1);
            var monthlyIncome = await _unitOfWork.Transactions.GetMonthlyIncomeAsync(null);
            var monthlySpending = await _unitOfWork.Transactions.GetMonthlySpendingAsync(null);

            // Create a DTO to hold the combined data
            var dashboardData = new DashboardDto
            {
                LatestBalance = latestBalance,
                Bills = bills,
                ExpectedIncome = expectedIncome,
                MonthlyIncome = monthlyIncome,
                MonthlySpending = monthlySpending
            };

            return Ok(dashboardData);
        }
    }
}