namespace BillsApi.Dtos
{
    public class UpdateTransactionDto
    {
        public required string Description { get; set; }
        
        public DateTime Date { get; set; }
        
        public string Status { get; set; } = "posted";
    }
}
