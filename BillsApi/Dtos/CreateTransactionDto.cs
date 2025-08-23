namespace BillsApi.Dtos
{
    public class CreateTransactionDto
    {
        
        public required string Description { get; set; }

        public decimal? Withdrawal { get; set; }

        public decimal? Deposit { get; set; }

        public DateTime Date { get; set; }

        public string? AccountName { get; set; }

        public string Status { get; set; } = "posted";

        public int GroupId { get; set; } = 1;
    }
}
