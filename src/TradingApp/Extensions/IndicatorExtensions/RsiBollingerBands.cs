using TradingApp.Models.DataTransferObjects;
using TradingApp.Models.Enums;
using TradingApp.Models.Indicators;

namespace TradingApp.Extensions.IndicatorExtensions;

public static partial class Indicator
{
    public static IndicatorResult[] CalcRsiBollingerBands(this Candle[] candles, int bbWindow = 20, int rsiWindow = 14, double stdDev = 2,
        double maxSpread = 0.0004, double minGain = 0.0006, double avgVolumeMultiplier = 1.5, double riskReward = 1.5,
        double rsiLowerBase = 20, double rsiUpperBase = 70, double volatilityAdjustmentFactor = 0.5)
    {
        var rsiResults = candles.CalcRsi(rsiWindow);
        var bollingerBands = candles.CalcBollingerBands(bbWindow, stdDev);
        var avgVolume = candles.Average(c => c.Volume);
        var length = candles.Length;
        var result = new IndicatorResult[length];
        double lastStopLoss = 0;

        for (var i = 0; i < length; i++)
        {
            result[i] ??= new IndicatorResult();
            result[i].Candle = candles[i];
            result[i].Gain = Math.Abs(candles[i].Mid_O - bollingerBands[i].Sma);

            double dynamicRsiLower = rsiLowerBase - candles[i].Volatility * volatilityAdjustmentFactor;
            double dynamicRsiUpper = rsiUpperBase + candles[i].Volatility * volatilityAdjustmentFactor;

            result[i].Signal = i == 0 ? Signal.None : candles[i] switch
            {
                var candle when candle.Mid_C > bollingerBands[i].LowerBand &&
                                candles[i - 1].Mid_C < bollingerBands[i - 1].LowerBand &&
                                rsiResults[i - 1].Rsi < dynamicRsiLower &&
                                candle.Spread <= maxSpread &&
                                candle.Volume >= avgVolume * avgVolumeMultiplier &&
                                result[i].Gain >= minGain => Signal.Buy,

                var candle when candle.Mid_C < bollingerBands[i].UpperBand &&
                                candles[i - 1].Mid_C > bollingerBands[i - 1].UpperBand &&
                                rsiResults[i - 1].Rsi > dynamicRsiUpper &&
                                candle.Spread <= maxSpread &&
                                candle.Volume >= avgVolume * avgVolumeMultiplier &&
                                result[i].Gain >= minGain => Signal.Sell,
                _ => Signal.None
            };

            if (result[i].Signal == Signal.Buy || result[i].Signal == Signal.Sell)
            {
                result[i].TakeProfit = candles[i].CalcTakeProfit(result[i]);
                result[i].StopLoss = result[i].Signal == Signal.Buy
                    ? candles[i].CalcStopLoss(result[i], riskReward)
                    : Math.Max(lastStopLoss, candles[i].CalcTrailingStopLoss(result[i], 0.5));
            }
            else
            {
                result[i].TakeProfit = 0;
                result[i].StopLoss = lastStopLoss; // Carry over the last stop loss if it's a trailing one
            }

            lastStopLoss = result[i].StopLoss; // Update the last stop loss to the new one if it was updated
            result[i].Loss = Math.Abs(candles[i].Mid_C - result[i].StopLoss);
        }

        return result;
    }
}