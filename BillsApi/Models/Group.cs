using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BillsApi.Models;

public partial class Group
{
    public int Id { get; set; }

    public string? Name { get; set; }

    [JsonIgnore]
    public virtual ICollection<AccountBalance> AccountBalances { get; set; } = new List<AccountBalance>();

    [JsonIgnore]
    public virtual ICollection<BillConfiguration> BillConfigurations { get; set; } = new List<BillConfiguration>();

    [JsonIgnore]
    public virtual ICollection<DailyAllowance> DailyAllowances { get; set; } = new List<DailyAllowance>();

    [JsonIgnore]
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    [JsonIgnore]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
