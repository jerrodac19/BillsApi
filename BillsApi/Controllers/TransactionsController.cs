using BillsApi.Dtos;
using BillsApi.Models;
using BillsApi.Repositories.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace BillsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetLastNTransactions([FromQuery] int n = 10, [FromQuery] string? accountName = null)
        {
            if (n <= 0)
            {
                return BadRequest("The number of transactions must be a positive integer.");
            }

            var transactions = await _unitOfWork.Transactions.GetLastNTransactionsAsync(n, accountName);

            return Ok(transactions);
        }

        [HttpGet("monthlyIncome")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetMonthlyIncome([FromQuery] string? accountName = null)
        {
            var transactions = await _unitOfWork.Transactions.GetMonthlyIncomeAsync(accountName);

            return Ok(transactions);
        }

        [HttpGet("monthlySpendingTotal")]
        public async Task<ActionResult<decimal>> GetMonthlySpendingTotal([FromQuery] string? accountName = null)
        {
            var totalAmountSpent = await _unitOfWork.Transactions.GetMonthlySpendingTotalAsync(accountName);

            return Ok(totalAmountSpent);
        }

        [HttpPost]
        public async Task<ActionResult<Transaction>> PostTransaction([FromBody] CreateTransactionDto createTransactionDto)
        {
            var transaction = new Transaction
            {
                Withdrawal = createTransactionDto.Withdrawal,
                Deposit = createTransactionDto.Deposit,
                Date = createTransactionDto.Date,
                Description = createTransactionDto.Description,
                Status = createTransactionDto.Status,
                AccountName = createTransactionDto.AccountName,
                GroupId = createTransactionDto.GroupId
            };

            await _unitOfWork.Transactions.AddAsync(transaction);
            await _unitOfWork.SaveAsync();

            return CreatedAtAction(nameof(GetLastNTransactions), new { id = transaction.Id }, transaction);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransaction(int id, UpdateTransactionDto transactionDto)
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            transaction.Description = transactionDto.Description;
            transaction.Date = transactionDto.Date;
            transaction.Status = transactionDto.Status;

            await _unitOfWork.Transactions.UpdateAsync(transaction);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }
    }
}