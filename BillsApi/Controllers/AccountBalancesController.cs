using BillsApi.Dtos;
using BillsApi.Models;
using BillsApi.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace BillsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountBalancesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AccountBalancesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("latest")]
        public async Task<ActionResult<IEnumerable<AccountBalance>>> GetLatestAccountBalance()
        {
            var latestBalance = await _unitOfWork.AccountBalances.GetLatestBalanceAsync();

            if (latestBalance == null)
            {
                return NotFound();
            }

            return Ok(latestBalance);
        }

        [HttpPost]
        public async Task<ActionResult<AccountBalance>> UpdateBalance([FromBody] CreateAccountBalanceDto createAccountBalanceDto)
        {
            var accountBalance = new AccountBalance
            {
                Amount = createAccountBalanceDto.Amount,
                Updated = createAccountBalanceDto.Updated,
                GroupId = createAccountBalanceDto.GroupId
            };

            await _unitOfWork.AccountBalances.AddAsync(accountBalance);
            await _unitOfWork.SaveAsync();

            return CreatedAtAction(nameof(GetLatestAccountBalance), new { id = accountBalance.Id }, accountBalance);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AccountBalance>>> GetLastNBalances([FromQuery] int n = 1)
        {
            if (n <= 0)
            {
                return BadRequest("The number of transactions must be a positive integer.");
            }

            var balances = await _unitOfWork.AccountBalances.GetLastNBalancesAsync(n);

            return Ok(balances);
        }
    }
}