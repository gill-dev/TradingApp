using System.Linq;
using TradingApp.Models.DataTransferObjects;
using TradingApp.Models.Enums;
using TradingApp.Models.Indicators;
using TradingApp.Extensions;

namespace TradingApp.Extensions.IndicatorExtensions;

public static partial class Indicator
{
    public static IndicatorResult[] CalcRsiEma(this Candle[] candles, int rsiWindow = 14, int emaWindow = 50,
        double rsiOverbought = 70, double rsiOversold = 30, double maxSpread = 0.0004, double minGain = 0.0006, double riskReward = 1.5,
        int resistanceLookBack = 20, double supportTolerance = 0.0005)
    {
        var rsiResults = candles.CalcRsi(rsiWindow);
        var prices = candles.Select(c => c.Mid_C).ToArray();
        var emaResults = prices.CalcEma(emaWindow).ToArray();
        var resistanceLevels = NumericExtensions.FindResistanceLevels(candles, resistanceLookBack);
        var atrResults = candles.CalcAtr().ToArray();

        var length = candles.Length;
        var results = new IndicatorResult[length];

        for (var i = 0; i < length; i++)
        {
            results[i] ??= new IndicatorResult();
            results[i].Candle = candles[i];

            var rsi = rsiResults[i].Rsi;
            var ema = emaResults[i];

            results[i].Gain = Math.Abs(candles[i].Mid_C - ema);

            bool supportConfirmed = resistanceLevels.Any(level => NumericExtensions.CheckIfSupport(candles.Take(i + 1).ToArray(), level, supportTolerance));

            // Adjusting the signal logic to include checks for resistance-turned-support
            results[i].Signal = ((candles[i].Mid_C > ema && rsi > rsiOverbought && supportConfirmed) &&
                                (candles[i].Spread <= maxSpread && results[i].Gain >= minGain)) switch
            {
                true => Signal.Buy,
                _ => ((candles[i].Mid_C < ema && rsi < rsiOversold) &&
                      (candles[i].Spread <= maxSpread && results[i].Gain >= minGain)) ? Signal.Sell : Signal.None
            };

            results[i].StopLoss = candles[i].CalcStopLoss(results[i], riskReward, atrResults[i]);
            results[i].TakeProfit = candles[i].CalcTakeProfit(results[i]);
            results[i].StopLoss = candles[i].CalcStopLoss(results[i], riskReward);
            results[i].Loss = Math.Abs(candles[i].Mid_C - results[i].StopLoss);
        }

        return results;
    }

}
