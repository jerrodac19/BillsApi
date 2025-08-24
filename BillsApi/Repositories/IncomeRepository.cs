namespace BillsApi.Repositories
{
    using BillsApi.Models;
    using BillsApi.Repositories.Common;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class IncomeRepository : GenericRepository<Income>, IIncomeRepository
    {
        public IncomeRepository(BillsApiContext context) : base(context) { }

        public async Task<IEnumerable<Income>> GetIncomesByGroupIdAsync(int groupId)
        {
            return await _context.Incomes
                .Include(i => i.User)
                .Where(i => i.User.GroupId == groupId)
                .ToListAsync();
        }
    }
}
