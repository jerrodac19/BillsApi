using System;
using System.Collections.Generic;

namespace BillsApi.Models;

public partial class BalanceMonitor
{
    public int Id { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? Updated { get; set; }
}
