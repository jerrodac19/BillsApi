namespace BillsApi.Repositories.UnitOfWork
{
    using BillsApi.Repositories;
    using System;
    using System.Threading.Tasks;

    public interface IUnitOfWork : IDisposable
    {
        IAccountBalanceRepository AccountBalances { get; }
        IBalanceMonitorRepository BalanceMonitors { get; }
        IBillConfigurationRepository BillConfigurations { get; }
        IBillRepository Bills { get; }
        IIncomeRepository Incomes { get; }
        ITransactionRepository Transactions { get; }
        Task<int> SaveAsync();
    }
}
