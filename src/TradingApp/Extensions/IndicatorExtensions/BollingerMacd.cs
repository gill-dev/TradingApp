using TradingApp.Models.DataTransferObjects;
using TradingApp.Models.Enums;
using TradingApp.Models.Indicators;

namespace TradingApp.Extensions.IndicatorExtensions
{
    public static partial class Indicator
    {
        public static IndicatorResult[] CalcBollingerMacdEma(this Candle[] candles, int bbWindow = 12, double stdDev = 2,
            int emaWindow = 100, double maxSpread = 0.0004, double minGain = 0.0006, double riskReward = 1.5)
        {
            var bollingerBands = candles.CalcBollingerBands(bbWindow, stdDev);
            var macdResults = candles.CalcMacd().ToArray();
            var prices = candles.Select(c => c.Mid_C).ToArray();
            var emaResult = prices.CalcEma(emaWindow).ToArray(); 

            var length = candles.Length;
            var result = new IndicatorResult[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = new IndicatorResult
                {
                    Candle = candles[i]
                };

                var currentMacd = macdResults[i];
                var currentEma = emaResult[i];
                var currentBollinger = bollingerBands[i];

                var macDelta = currentMacd.Macd - currentMacd.SignalLine;
                var macDeltaPrev = i == 0 ? 0 : macdResults[i - 1].Macd - macdResults[i - 1].SignalLine;

                int direction = macDelta > 0 && macDeltaPrev < 0 ? 1 :
                    macDelta < 0 && macDeltaPrev > 0 ? -1 : 0;

                result[i].Gain = Math.Abs(candles[i].Mid_C - currentEma);

                result[i].Signal = (candles[i], direction) switch
                {
                    (var candle, 1) when candle.Mid_L > currentEma && candle.Mid_C < currentBollinger.LowerBand && candle.Spread <= maxSpread && result[i].Gain >= minGain => Signal.Buy,
                    (var candle, -1) when candle.Mid_H < currentEma && candle.Mid_C > currentBollinger.UpperBand && candle.Spread <= maxSpread && result[i].Gain >= minGain => Signal.Sell,
                    _ => Signal.None
                };

                result[i].TakeProfit = candles[i].CalcTakeProfit(result[i]);
                result[i].StopLoss = candles[i].CalcStopLoss(result[i], riskReward);
                result[i].Loss = Math.Abs(candles[i].Mid_C - result[i].StopLoss);
            }

            return result;
        }



    }
}
