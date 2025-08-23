using System;
using System.Collections.Generic;

namespace BillsApi.Models;

public partial class AccountBalance
{
    public int Id { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? Updated { get; set; }

    public int GroupId { get; set; }

    public virtual Group Group { get; set; } = null!;
}
