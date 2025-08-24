namespace BillsApi.Repositories
{
    using BillsApi.Models;
    using BillsApi.Repositories.Common;

    public class BillConfigurationRepository : GenericRepository<BillConfiguration>, IBillConfigurationRepository
    {
        public BillConfigurationRepository(BillsApiContext context) : base(context) { }
    }
}
