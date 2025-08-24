namespace BillsApi.Repositories
{
    using BillsApi.Models;
    using BillsApi.Repositories.Common;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IAccountBalanceRepository : IRepository<AccountBalance>
    {
        Task<AccountBalance?> GetLatestBalanceAsync();
        Task<IEnumerable<AccountBalance>> GetLastNBalancesAsync(int n);
    }
}
