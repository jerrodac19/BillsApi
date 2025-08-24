namespace BillsApi.Repositories
{
    using BillsApi.Models;
    using BillsApi.Repositories.Common;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class AccountBalanceRepository : GenericRepository<AccountBalance>, IAccountBalanceRepository
    {
        public AccountBalanceRepository(BillsApiContext context) : base(context) { }

        public async Task<AccountBalance?> GetLatestBalanceAsync()
        {
            return await _context.AccountBalances
                .OrderByDescending(ab => ab.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AccountBalance>> GetLastNBalancesAsync(int n)
        {
            return await _context.AccountBalances
                .OrderByDescending(ab => ab.Id)
                .Take(n)
                .ToListAsync();
        }
    }
}