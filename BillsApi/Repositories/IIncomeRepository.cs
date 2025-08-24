namespace BillsApi.Repositories
{
    using BillsApi.Models;
    using BillsApi.Repositories.Common;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IIncomeRepository : IRepository<Income>
    {
        Task<IEnumerable<Income>> GetIncomesByGroupIdAsync(int groupId);
    }
}