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
            var arizonaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Phoenix");

            // Fetch all relevant data from the database first
            var allData = await _context.BalanceMonitors
                .Where(b => b.Updated.HasValue)
                .OrderBy(b => b.Updated)
                .ToListAsync(); // The async operation happens here

            // Perform in-memory grouping using the local date
            return allData
                .GroupBy(b => TimeZoneInfo.ConvertTimeFromUtc(b.Updated!.Value, arizonaTimeZone).Date)
                .Select(g => g.LastOrDefault()!);
        }
    }
}