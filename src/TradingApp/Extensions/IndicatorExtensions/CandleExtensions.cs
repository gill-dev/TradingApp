using TradingApp.Models.DataTransferObjects;

namespace TradingApp.Extensions.IndicatorExtensions
{
    public static class CandleExtensions
    {
        public static void CalculateVolatility(this List<Candle> candles, int windowSize = 20)
        {
            if (candles.Count < windowSize) return; // Not enough data to calculate

            double[] returns = new double[candles.Count];

            // Calculate returns as percentage change from previous close
            for (int i = 1; i < candles.Count; i++)
            {
                returns[i] = (candles[i].Mid_C - candles[i - 1].Mid_C) / candles[i - 1].Mid_C;
            }

            // Calculate the rolling standard deviation of returns
            for (int i = windowSize; i < candles.Count; i++)
            {
                double mean = returns.Skip(i - windowSize).Take(windowSize).Average();
                double sumOfSquaresOfDifferences = returns.Skip(i - windowSize).Take(windowSize).Select(val => (val - mean) * (val - mean)).Sum();
                double standardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / windowSize);
                candles[i].Volatility = standardDeviation;
            }
        }
    }
}