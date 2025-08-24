using BillsApi.Models;
using System.Collections.Generic;

namespace BillsApi.Dtos
{
    public class DashboardDto
    {
        public AccountBalance? LatestBalance { get; set; }
        public IEnumerable<Bill>? Bills { get; set; }
        public IEnumerable<Income>? ExpectedIncome { get; set; }
        public IEnumerable<Transaction>? MonthlyIncome { get; set; }
    }
}
