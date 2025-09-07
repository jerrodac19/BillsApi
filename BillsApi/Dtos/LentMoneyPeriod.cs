namespace BillsApi.Dtos
{
    public class LentMoneyPeriod
    {
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
