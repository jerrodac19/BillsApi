namespace BillsApi.Repositories
{
    using BillsApi.Models;
    using BillsApi.Repositories.Common;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ITransactionRepository : IRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetLastNTransactionsAsync(int n, string? accountName);
        Task<IEnumerable<Transaction>> GetMonthlyIncomeAsync(string? accountName);
        Task<decimal> GetMonthlySpendingAsync(string? accountName);
    }
}