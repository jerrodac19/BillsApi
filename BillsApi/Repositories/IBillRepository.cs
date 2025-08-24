namespace BillsApi.Repositories
{
    using BillsApi.Models;
    using BillsApi.Repositories.Common;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IBillRepository : IRepository<Bill>
    {
        Task<IEnumerable<Bill>> GetBillsWithConfigurationAsync(string? filter);
        Task<Bill?> GetBillWithConfigurationByIdAsync(int id);
        Task<bool> BillExistsAsync(int id);
    }
}