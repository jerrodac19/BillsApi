namespace BillsApi.Repositories
{
    using BillsApi.Models;
    using BillsApi.Repositories.Common;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        public TransactionRepository(BillsApiContext context) : base(context) { }

        public async Task<IEnumerable<Transaction>> GetLastNTransactionsAsync(int n, string? accountName)
        {
            var query = _context.Transactions.AsQueryable();
            if (!string.IsNullOrEmpty(accountName))
            {
                query = query.Where(t => t.AccountName != null && t.AccountName == accountName);
            }
            return await query.OrderByDescending(t => t.Id).Take(n).ToListAsync();
        }

        public async Task<IEnumerable<Transaction>> GetMonthlyIncomeAsync(string? accountName)
        {
            var query = _context.Transactions.AsQueryable();
            DateTime utcNow = DateTime.UtcNow;
            DateTime firstDayOfMonth = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            query = query.Where(t => t.Deposit > 0 && t.CreationTime != null && t.CreationTime >= firstDayOfMonth);

            if (!string.IsNullOrEmpty(accountName))
            {
                query = query.Where(t => t.AccountName != null && t.AccountName == accountName);
            }
            return await query.OrderByDescending(t => t.Id).ToListAsync();
        }

        public async Task<decimal> GetMonthlySpendingTotalAsync(string? accountName)
        {
            var query = _context.Transactions.AsQueryable();
            DateTime utcNow = DateTime.UtcNow;
            DateTime firstDayOfMonth = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            query = query.Where(t => t.Withdrawal > 0 && t.CreationTime != null && t.CreationTime >= firstDayOfMonth);

            if (!string.IsNullOrEmpty(accountName))
            {
                query = query.Where(t => t.AccountName != null && t.AccountName == accountName);
            }

            var totalSpending = await query.SumAsync(t => t.Withdrawal);
            return (decimal)totalSpending;
        }
    }
}
