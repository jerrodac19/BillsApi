namespace BillsApi.Dtos
{
    public class BalanceAnalyticsResult
    {
        public required List<List<double>> HistoricalData { get; set; }
        public required List<List<double>> Trendline { get; set; }
        public required List<List<double>> LowerPredictionInterval { get; set; }
        public required List<List<double>> UpperPredictionInterval { get; set; }
        public double OriginalXIntercept { get; set; }
        public double EarliestRunOutDateX { get; set; }
        public double LatestRunOutDateX { get; set; }
        public double RSquared { get; set; }
        public double Ser { get; set; }
        public double Confidence { get; set; }
        public long StartTimeUnix { get; set; }
    }
}
