namespace BillsApi.Repositories
{
    using BillsApi.Models;
    using BillsApi.Repositories.Common;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IBalanceMonitorRepository : IRepository<BalanceMonitor>
    {
        Task<IEnumerable<BalanceMonitor>> GetLatestDailyBalancesAsync();
    }
}
