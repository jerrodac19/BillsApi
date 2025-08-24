namespace BillsApi.Repositories.UnitOfWork
{
    using BillsApi.Models;
    using BillsApi.Repositories;
    using System;
    using System.Threading.Tasks;

    public class UnitOfWork : IUnitOfWork
    {
        private readonly BillsApiContext _context;

        public IAccountBalanceRepository AccountBalances { get; private set; }
        public IBalanceMonitorRepository BalanceMonitors { get; private set; }
        public IBillConfigurationRepository BillConfigurations { get; private set; }
        public IBillRepository Bills { get; private set; }
        public IIncomeRepository Incomes { get; private set; }
        public ITransactionRepository Transactions { get; private set; }

        public UnitOfWork(BillsApiContext context)
        {
            _context = context;
            AccountBalances = new AccountBalanceRepository(_context);
            BalanceMonitors = new BalanceMonitorRepository(_context);
            BillConfigurations = new BillConfigurationRepository(_context);
            Bills = new BillRepository(_context);
            Incomes = new IncomeRepository(_context);
            Transactions = new TransactionRepository(_context);
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
