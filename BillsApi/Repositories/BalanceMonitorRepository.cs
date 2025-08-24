namespace BillsApi.Repositories
{
    using BillsApi.Models;
    using BillsApi.Repositories.Common;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class BalanceMonitorRepository : GenericRepository<BalanceMonitor>, IBalanceMonitorRepository
    {
        public BalanceMonitorRepository(BillsApiContext context) : base(context) { }

        public async Task<IEnumerable<BalanceMonitor>> GetLatestDailyBalancesAsync()
        {
            return await _context.BalanceMonitors
                .Where(b => b.Updated.HasValue)
                .GroupBy(b => b.Updated!.Value.Date)
                .Select(g => g.OrderByDescending(b => b.Updated).FirstOrDefault()!) // Use ! to assert non-nullability
                .ToListAsync();
        }
    }
}