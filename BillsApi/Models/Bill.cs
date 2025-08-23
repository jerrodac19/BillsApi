using System;
using System.Collections.Generic;

namespace BillsApi.Models;

public partial class Bill
{
    public int Id { get; set; }

    public decimal? Amount { get; set; }

    public DateOnly? DueDate { get; set; }

    public bool? Payed { get; set; }

    public short? PayEarly { get; set; }

    public bool? Active { get; set; }

    public DateTime Updated { get; set; }

    public int ConfigurationId { get; set; }

    public bool Valid { get; set; }

    public string? CalendarEventId { get; set; }

    public string? ReminderId { get; set; }

    public string? TaskId { get; set; }

    public string? Title { get; set; }

    public virtual BillConfiguration Configuration { get; set; } = null!;
}
