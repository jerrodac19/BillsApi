namespace BillsApi.Dtos
{
    public class CreateAccountBalanceDto
    {
        public decimal? Amount { get; set; }

        public DateTime Updated { get; set; } = DateTime.UtcNow;

        public int GroupId { get; set; } = 1;
    }
}
