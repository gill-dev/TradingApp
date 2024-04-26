using TradingApp.Models.DataTransferObjects;
using TradingApp.Models.Enums;
using TradingApp.Models.Indicators;

namespace TradingApp.Extensions.IndicatorExtensions;

public static partial class Indicator
{
    public static IndicatorResult[] CalcMacdEmaRsi(this Candle[] candles, int emaWindow = 20, int rsiPeriod = 14, double maxSpread = 0.0004, double minGain = 0.0006, double riskReward = 1.5)
    {
        var macd = candles.CalcMacd();
        var rsi = candles.CalcRsi(rsiPeriod);
        var prices = candles.Select(c => c.Mid_C).ToArray();
        var emaResult = prices.CalcEma(emaWindow).ToArray();

        var length = candles.Length;
        var result = new IndicatorResult[length];

        for (var i = 0; i < length; i++)
        {
            result[i] ??= new IndicatorResult();
            result[i].Candle = candles[i];
            var macDelta = macd[i].Macd - macd[i].SignalLine;
            var macDeltaPrev = i == 0 ? 0.0 : macd[i - 1].Macd - macd[i - 1].SignalLine;

            var direction = macDelta switch
            {
                > 0 when macDeltaPrev < 0 => 1,
                < 0 when macDeltaPrev > 0 => -1,
                _ => 0
            };

            var ema = emaResult[i];
            result[i].Gain = Math.Abs(candles[i].Mid_C - ema);
            var rsiCondition = rsi[i].Rsi < 30 || rsi[i].Rsi > 70; // Check using the Rsi property

            result[i].Signal = direction switch
            {
                1 when candles[i].Mid_L > ema && candles[i].Spread <= maxSpread &&
                       result[i].Gain >= minGain && rsiCondition => Signal.Buy,
                -1 when candles[i].Mid_H < ema && candles[i].Spread <= maxSpread &&
                        result[i].Gain >= minGain && rsiCondition => Signal.Sell,
                _ => Signal.None
            };

            result[i].TakeProfit = candles[i].CalcTakeProfit(result[i]);
            result[i].StopLoss = candles[i].CalcStopLoss(result[i], riskReward);
            result[i].Loss = Math.Abs(candles[i].Mid_C - result[i].StopLoss);
        }

        return result;
    }


}