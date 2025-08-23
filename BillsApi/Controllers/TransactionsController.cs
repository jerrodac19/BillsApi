using BillsApi.Dtos;
using BillsApi.Models;
using BillsApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly BillsApiContext _context; // Your DbContext

        public TransactionsController(BillsApiContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the last N transactions from the database.
        /// </summary>
        /// <param name="n">The number of transactions to return (e.g., 10). Defaults to 10 if not provided.</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetLastNTransactions([FromQuery] int n = 10, [FromQuery] string? accountName = null)
        {
            if (n <= 0)
            {
                return BadRequest("The number of transactions must be a positive integer.");
            }

            var query = _context.Transactions.AsQueryable();

            if (!string.IsNullOrEmpty(accountName))
            {
                query = query.Where(t => t.AccountName != null && t.AccountName == accountName);
            }

            var transactions = await query.OrderByDescending(t => t.Id).Take(n).ToListAsync();

            return Ok(transactions);
        }

        /// <summary>
        /// Gets the all income transactions for the current month.
        /// </summary>
        [HttpGet("monthlyIncome")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetMonthlyIncome([FromQuery] string? accountName = null)
        {
            var query = _context.Transactions.AsQueryable();

            // Get the current date and time
            DateTime today = DateTime.Today;

            // Create a new DateTime object representing the first day of the current month
            DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

            query = query.Where(t => t.Deposit > 0 && t.Date != null && t.Date >= firstDayOfMonth);

            if (!string.IsNullOrEmpty(accountName))
            {
                query = query.Where(t => t.AccountName != null && t.AccountName == accountName);
            }

            var transactions = await query.OrderByDescending(t => t.Id).ToListAsync();

            return Ok(transactions);
        }

        /// <summary>
        /// Creates a new transaction.
        /// </summary>
        /// <param name="createTransactionDto">The data for the new transaction.</param>
        [HttpPost]
        public async Task<ActionResult<Transaction>> PostTransaction([FromBody] CreateTransactionDto createTransactionDto)
        {
            // Map the DTO to your Transaction entity
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

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // Return a 201 Created status with the newly created transaction
            return CreatedAtAction(nameof(GetLastNTransactions), new { id = transaction.Id }, transaction);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransaction(int id, UpdateTransactionDto transactionDto)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            // Map the properties from the DTO to the entity
            
            transaction.Description = transactionDto.Description;
            transaction.Date = transactionDto.Date;
            transaction.Status = transactionDto.Status;

            await _context.SaveChangesAsync();

            // A successful update typically returns a 204 No Content status
            return NoContent();
        }
    }
}
