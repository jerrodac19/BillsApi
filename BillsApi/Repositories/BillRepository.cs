namespace BillsApi.Repositories
{
    using BillsApi.Models;
    using BillsApi.Repositories.Common;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class BillRepository : GenericRepository<Bill>, IBillRepository
    {
        public BillRepository(BillsApiContext context) : base(context) { }

        public async Task<IEnumerable<Bill>> GetBillsWithConfigurationAsync(string? filter)
        {
            var query = _context.Bills.Include(b => b.Configuration).AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(b => b.Title != null && b.Title.Contains(filter) && b.Active == true);
            }
            else
            {
                query = query.Where(b => b.Active == true);
            }

            return await query.ToListAsync();
        }

        public async Task<Bill?> GetBillWithConfigurationByIdAsync(int id)
        {
            return await _context.Bills.Include(b => b.Configuration).FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> BillExistsAsync(int id)
        {
            return await _context.Bills.AnyAsync(e => e.Id == id);
        }
    }
}