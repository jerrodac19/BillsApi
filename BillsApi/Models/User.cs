using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BillsApi.Models;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? GroupId { get; set; }

    public virtual Group? Group { get; set; }

    [JsonIgnore]
    public virtual ICollection<Income> Incomes { get; set; } = new List<Income>();
}
