using System;
using System.Collections.Generic;

namespace BillsApi.Models;

public partial class Transaction
{
    public decimal? Withdrawal { get; set; }

    public decimal? Deposit { get; set; }

    public string? Description { get; set; }

    public int Id { get; set; }

    public int GroupId { get; set; }

    public DateTime? Date { get; set; }

    public string? AccountName { get; set; }

    public string? Status { get; set; }

    public virtual Group Group { get; set; } = null!;
}
