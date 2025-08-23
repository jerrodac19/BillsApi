using BillsApi.Dtos;
using BillsApi.Models;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearRegression;
using MathNet.Numerics.Statistics;

public class BalanceAnalyticsService
{
    public BalanceAnalyticsResult Analyze(IEnumerable<BalanceMonitor> data)
    {
        // Get the Unix timestamp from the first data point
        var firstUpdate = data.First().Updated;
        long startTimeUnix = 0;
        if (firstUpdate != null)
        {
            startTimeUnix = new DateTimeOffset((DateTime)firstUpdate).ToUnixTimeSeconds();
        }

        if (data.Count() < 3 || firstUpdate == null)
        {
            throw new InvalidOperationException("Not enough data points for robust prediction intervals (minimum 3 required).");
        }

        var dataPoints = data
            .OrderBy(b => b.Updated)
            .ToList();
        
        var x_vals = dataPoints.Select(dp => ((DateTime)(dp.Updated!.Value) - (DateTime)firstUpdate).TotalDays).ToArray();
        var y_vals = dataPoints.Select(dp => (double)dp.Amount!).ToArray();

        // Correct way to perform linear regression. Fit() is a static method
        var (intercept, slope) = SimpleRegression.Fit(x_vals, y_vals);

        // Calculate predicted values for the historical data points
        var y_predicted_historical = x_vals.Select(x => slope * x + intercept).ToArray();

        // Correct way to get R-squared
        double rSquared = GoodnessOfFit.RSquared(y_vals, y_predicted_historical);

        double confidenceLevel = 0.99;
        int n = x_vals.Length;
        int df_residuals = n - 2;

        // Calculate Mean Square Error manually or from residuals
        double sumOfSquaredResiduals = y_vals.Zip(y_predicted_historical, (actual, predicted) => Math.Pow(actual - predicted, 2)).Sum();
        double mse = sumOfSquaredResiduals / df_residuals;
        double ser = Math.Sqrt(mse);

        double x_mean = x_vals.Average();
        double sum_sq_x_minus_mean = x_vals.Sum(x => Math.Pow(x - x_mean, 2));

        double original_x_intercept = slope != 0 ? -intercept / slope : double.PositiveInfinity;

        double start_x_for_plot = x_vals.Min();
        double estimated_max_project_x = Math.Max(original_x_intercept, x_vals.Max() + 30);
        if (slope < 0)
        {
            estimated_max_project_x = Math.Max(estimated_max_project_x, original_x_intercept + 0.5 * Math.Abs(original_x_intercept));
            estimated_max_project_x = Math.Max(estimated_max_project_x, x_vals.Max() + 100);
        }
        else
        {
            estimated_max_project_x = x_vals.Max() + 100;
        }

        var all_x_for_plot = Enumerable.Range(0, 1000)
            .Select(i => start_x_for_plot + i * (estimated_max_project_x - start_x_for_plot) / 999.0)
            .ToArray();

        var y_predicted_all = all_x_for_plot.Select(x => slope * x + intercept).ToList();

        var lower_pi_vals = new List<double>();
        var upper_pi_vals = new List<double>();
        double alpha_val = 1 - confidenceLevel;

        double critical_t = 0;
        if (df_residuals > 0)
        {
            critical_t = StudentT.InvCDF(0, 1, df_residuals, 1 - alpha_val / 2);
        }

        foreach (var x_new in all_x_for_plot)
        {
            double se_pred_single = ser * Math.Sqrt(1 + (1.0 / n) + (Math.Pow(x_new - x_mean, 2) / sum_sq_x_minus_mean));
            double y_pred_single = slope * x_new + intercept;

            if (critical_t == 0)
            {
                lower_pi_vals.Add(y_pred_single);
                upper_pi_vals.Add(y_pred_single);
            }
            else
            {
                lower_pi_vals.Add(y_pred_single - critical_t * se_pred_single);
                upper_pi_vals.Add(y_pred_single + critical_t * se_pred_single);
            }
        }

        double earliest_run_out = double.PositiveInfinity;
        double latest_run_out = double.PositiveInfinity;
        double max_x_val = x_vals.Max();

        var search_points = all_x_for_plot
            .Zip(lower_pi_vals, (x, y) => (X: x, LowerPI: y))
            .Where(p => p.X >= max_x_val);

        var earliest_run_out_point = search_points.FirstOrDefault(p => p.LowerPI <= 0);
        if (earliest_run_out_point.X > 0) earliest_run_out = earliest_run_out_point.X;

        var latest_run_out_point = all_x_for_plot
            .Zip(upper_pi_vals, (x, y) => (X: x, UpperPI: y))
            .Where(p => p.X >= max_x_val)
            .FirstOrDefault(p => p.UpperPI <= 0);
        if (latest_run_out_point.X > 0) latest_run_out = latest_run_out_point.X;

        if (earliest_run_out > latest_run_out && latest_run_out != double.PositiveInfinity)
        {
            (earliest_run_out, latest_run_out) = (latest_run_out, earliest_run_out);
        }

        return new BalanceAnalyticsResult
        {
            HistoricalData = x_vals.Zip(y_vals, (x, y) => new List<double> { x, y }).ToList(),
            Trendline = all_x_for_plot.Zip(y_predicted_all, (x, y) => new List<double> { x, y }).ToList(),
            LowerPredictionInterval = all_x_for_plot.Zip(lower_pi_vals, (x, y) => new List<double> { x, y }).ToList(),
            UpperPredictionInterval = all_x_for_plot.Zip(upper_pi_vals, (x, y) => new List<double> { x, y }).ToList(),
            OriginalXIntercept = original_x_intercept,
            EarliestRunOutDateX = earliest_run_out,
            LatestRunOutDateX = latest_run_out,
            RSquared = rSquared,
            Ser = ser,
            Confidence = confidenceLevel,
            StartTimeUnix = startTimeUnix
        };
    }
}