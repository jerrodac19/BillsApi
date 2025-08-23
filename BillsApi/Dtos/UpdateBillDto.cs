namespace BillsApi.Models
{
    public class UpdateBillDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateOnly? DueDate { get; set; }
        public bool? Payed { get; set; }
        public short PayEarly { get; set; }
        public bool? Active { get; set; } = true;
        public int ConfigurationId { get; set; }
        public bool? Valid { get; set; }
        public string? CalendarEventId { get; set; }
        public string? ReminderId { get; set; }
        public string? TaskId { get; set; }
        public string? Title { get; set; }
    }
}
