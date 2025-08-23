using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BillsApi.Models;

public partial class BillConfiguration
{
    public int Id { get; set; }

    public decimal? DefaultAmount { get; set; }

    public short? DefaultPayEarly { get; set; }

    public string? Website { get; set; }

    public int? GroupId { get; set; }

    public int? MonthlyDueDate { get; set; }

    public string? DefaultTitle { get; set; }

    public string? TransactionRegex { get; set; }

    [JsonIgnore]
    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual Group? Group { get; set; }
}
