namespace BillsApi.Services
{
    using BillsApi.Dtos;
    using BillsApi.Models;
    using MathNet.Numerics;
    using MathNet.Numerics.Distributions;
    using MathNet.Numerics.LinearRegression;
    using MathNet.Numerics.Statistics;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class BalanceAnalyticsService
    {
        public BalanceAnalyticsResult Analyze(IEnumerable<BalanceMonitor> data, double confidenceLevel = 0.99, double weightDecayFactor = 0)
        {
            var firstUpdate = DateTime.SpecifyKind(data.First().Updated!.Value, DateTimeKind.Utc);
            long startTimeUnix = new DateTimeOffset(firstUpdate).ToUnixTimeSeconds();

            if (data.Count() < 3)
            {
                throw new InvalidOperationException("Not enough data points for robust prediction intervals (minimum 3 required).");
            }

            var x_vals = data.Select(dp => ((DateTime)(dp.Updated!.Value) - (DateTime)firstUpdate).TotalDays).ToArray();
            var y_vals = data.Select(dp => (double)dp.Amount!).ToArray();

            // --- WEIGHTED REGRESSION IMPLEMENTATION ---
            double alpha = weightDecayFactor; // Decay rate. This is a tunable parameter.
            double max_x = x_vals.Max();
            var weights = x_vals.Select(x => Math.Exp(-alpha * (max_x - x))).ToArray();
            double sumOfWeights = weights.Sum();

            var (intercept, slope) = Fit(x_vals, y_vals, weights);

            if (slope >= 0)
            {
                throw new InvalidOperationException("This calculation is only intended for negatively trending data, the given data isn't trending negative");
            }

            if (y_vals.First() <= 0)
            {
                throw new InvalidOperationException("This calculation is only intended for a positive starting balance.");
            }

            var y_predicted_historical = x_vals.Select(x => slope * x + intercept).ToArray();

            // R-squared is not directly applicable to weighted regression,
            // as its interpretation changes. For simplicity, we can use the unweighted R-squared,
            // but it's important to note its limitation.
            double rSquared = GoodnessOfFit.RSquared(y_vals, y_predicted_historical);

            int n = x_vals.Length;
            int df_residuals = n - 2;

            double sumOfSquaredResiduals = y_vals.Zip(y_predicted_historical, (actual, predicted) => Math.Pow(actual - predicted, 2)).Sum();
            double mse = sumOfSquaredResiduals / df_residuals;
            double ser = Math.Sqrt(mse);

            // Using weighted mean for prediction interval calculation
            double weightedSumX = x_vals.Zip(weights, (xi, w) => xi * w).Sum();
            double weightedMeanX = weightedSumX / sumOfWeights;
            double sum_weighted_sq_x_minus_mean = x_vals.Zip(weights, (x, w) => w * Math.Pow(x - weightedMeanX, 2)).Sum();

            double original_x_intercept = -intercept / slope;
            double start_x_for_plot = x_vals.Min();

            double estimated_max_project_x = Math.Max(max_x + 100, original_x_intercept * 1.5);

            const int numPlotPoints = 50;
            var plot_x_vals = Enumerable.Range(0, numPlotPoints)
                .Select(i => start_x_for_plot + i * (estimated_max_project_x - start_x_for_plot) / (numPlotPoints - 1))
                .ToArray();

            var trendline_points = new List<List<double>> {
                new List<double> { x_vals.Min(), slope * x_vals.Min() + intercept },
                new List<double> { estimated_max_project_x, slope * estimated_max_project_x + intercept }
            };

            var lower_pi_vals_plot = new List<double>();
            var upper_pi_vals_plot = new List<double>();
            double alpha_val = 1 - confidenceLevel;
            double critical_t = (df_residuals > 0) ? StudentT.InvCDF(0, 1, df_residuals, 1 - alpha_val / 2) : 0;

            Func<double, double> lowerPiFunction = x_new =>
            {
                double w_new;
                if (x_new <= max_x)
                {
                    // Use the original exponential decay for historical points
                    w_new = Math.Exp(-alpha * (max_x - x_new));
                }
                else
                {
                    // For future predictions, the weight should not grow. Set to a constant value.
                    // The value 1.0 is used here as a logical baseline.
                    w_new = 1.0;
                }
                double se_pred_single = ser * Math.Sqrt((1.0 / w_new) + (1.0 / sumOfWeights) + (Math.Pow(x_new - weightedMeanX, 2) / sum_weighted_sq_x_minus_mean));
                double y_pred_single = slope * x_new + intercept;
                return y_pred_single - critical_t * se_pred_single;
            };

            Func<double, double> upperPiFunction = x_new =>
            {
                double w_new;
                if (x_new <= max_x)
                {
                    // Use the original exponential decay for historical points
                    w_new = Math.Exp(-alpha * (max_x - x_new));
                }
                else
                {
                    // For future predictions, the weight should not grow. Set to a constant value.
                    // The value 1.0 is used here as a logical baseline.
                    w_new = 1.0;
                }
                double se_pred_single = ser * Math.Sqrt((1.0 / w_new) + (1.0 / sumOfWeights) + (Math.Pow(x_new - weightedMeanX, 2) / sum_weighted_sq_x_minus_mean));
                double y_pred_single = slope * x_new + intercept;
                return y_pred_single + critical_t * se_pred_single;
            };

            foreach (var x_new in plot_x_vals)
            {
                lower_pi_vals_plot.Add(lowerPiFunction(x_new));
                upper_pi_vals_plot.Add(upperPiFunction(x_new));
            }

            double earliest_run_out = FindRootWithBisection(lowerPiFunction, max_x, estimated_max_project_x, 0.001);
            double latest_run_out = FindRootWithBisection(upperPiFunction, max_x, estimated_max_project_x, 0.001);

            // --- REGENERATED LOGIC FOR PLOTTING HISTORICAL DATA ---
            int reduceVisualPointFactor = 5;
            var historical_data = new List<List<double>>();
            var historicalWeights = new List<double>();
            for (int i = 0; i < x_vals.Length; i += reduceVisualPointFactor)
            {
                historical_data.Add(new List<double> { x_vals[i], y_vals[i] });
                historicalWeights.Add(weights[i]);
            }

            // Always add the last point to ensure the plot is accurate to the latest data
            if ((x_vals.Length - 1) % reduceVisualPointFactor != 0)
            {
                historical_data.Add(new List<double> { x_vals.Last(), y_vals.Last() });
                historicalWeights.Add(weights.Last());
            }

            return new BalanceAnalyticsResult
            {
                HistoricalData = historical_data,
                HistoricalWeights = historicalWeights,
                Trendline = trendline_points,
                LowerPredictionInterval = plot_x_vals.Zip(lower_pi_vals_plot, (x, y) => new List<double> { x, y }).ToList(),
                UpperPredictionInterval = plot_x_vals.Zip(upper_pi_vals_plot, (x, y) => new List<double> { x, y }).ToList(),
                OriginalXIntercept = original_x_intercept,
                EarliestRunOutDateX = earliest_run_out,
                LatestRunOutDateX = double.IsInfinity(latest_run_out) ? 1000 : (double)latest_run_out,
                RSquared = rSquared,
                Ser = ser,
                Confidence = confidenceLevel,
                StartTimeUnix = startTimeUnix
            };
        }

        public IEnumerable<BalanceMonitor> NormalizeLentMoney(IEnumerable<BalanceMonitor> data, decimal amountLent, DateTime startDate, DateTime endDate)
        {
            var normalizedData = new List<BalanceMonitor>();

            foreach (var point in data)
            {
                if (point.Updated >= startDate && point.Updated <= endDate)
                {
                    normalizedData.Add(new BalanceMonitor
                    {
                        Id = point.Id,
                        Amount = point.Amount + amountLent,
                        Updated = point.Updated
                    });
                }
                else
                {
                    normalizedData.Add(new BalanceMonitor
                    {
                        Id = point.Id,
                        Amount = point.Amount,
                        Updated = point.Updated
                    });
                }
            }
            return normalizedData;
        }

        public double FindRootWithBisection(Func<double, double> function, double lowerBound, double upperBound, double tolerance)
        {
            double f_lower = function(lowerBound);
            double f_upper = function(upperBound);

            // Check if the root is within the initial interval
            if (f_lower * f_upper >= 0)
            {
                return double.PositiveInfinity; // No root found in interval
            }

            double midpoint = 0;
            // Iterate until the interval is smaller than the tolerance
            while ((upperBound - lowerBound) >= tolerance)
            {
                midpoint = (lowerBound + upperBound) / 2;
                double f_mid = function(midpoint);

                if (f_mid == 0)
                {
                    return midpoint;
                }
                else if (f_mid * f_lower < 0)
                {
                    upperBound = midpoint;
                }
                else
                {
                    lowerBound = midpoint;
                }
            }

            return (lowerBound + upperBound) / 2;
        }

        public static (double intercept, double slope) Fit(double[] x, double[] y, double[] weights)
        {
            if (x.Length != y.Length || x.Length != weights.Length)
            {
                throw new ArgumentException("Arrays must have the same length.");
            }

            double sumOfWeights = weights.Sum();
            double weightedSumX = x.Zip(weights, (xi, w) => xi * w).Sum();
            double weightedSumY = y.Zip(weights, (yi, w) => yi * w).Sum();
            double weightedMeanX = weightedSumX / sumOfWeights;
            double weightedMeanY = weightedSumY / sumOfWeights;

            double numerator = 0;
            double denominator = 0;
            for (int i = 0; i < x.Length; i++)
            {
                numerator += weights[i] * (x[i] - weightedMeanX) * (y[i] - weightedMeanY);
                denominator += weights[i] * Math.Pow(x[i] - weightedMeanX, 2);
            }

            double slope = numerator / denominator;
            double intercept = weightedMeanY - slope * weightedMeanX;

            return (intercept, slope);
        }
    }
}