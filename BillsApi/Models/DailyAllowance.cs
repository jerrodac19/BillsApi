using System;
using System.Collections.Generic;

namespace BillsApi.Models;

public partial class DailyAllowance
{
    public int Id { get; set; }

    public decimal? Allowance { get; set; }

    public DateTime? Date { get; set; }

    public int GroupId { get; set; }

    public virtual Group Group { get; set; } = null!;
}
