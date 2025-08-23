using System;
using System.Collections.Generic;

namespace BillsApi.Models;

public partial class Income
{
    public int Id { get; set; }

    public DateOnly StartDate { get; set; }

    public int? Frequency { get; set; }

    public decimal Amount { get; set; }

    public string? SearchString { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
