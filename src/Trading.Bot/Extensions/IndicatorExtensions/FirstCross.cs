using Trading.Bot.Extensions;
using System.Linq;

namespace Trading.Bot.Extensions.IndicatorExtensions;

public static partial class Indicator
{
    public static IndicatorResult[] CalculateFirstCrossSignals(this Candle[] candles, int bbWindow = 20, int emaWindow = 100,
       double stdDev = 2, double maxSpread = 0.0004, double minGain = 0.0006, int minVolume = 100, double riskReward = 1.5)
    {
        var prices = candles.Select(c => c.Mid_C).ToArray();
        var oscillator = prices.Calc310Oscillator().ToArray(); 
        var ma16 = oscillator.CalcSma(16).ToArray();

        var emaResult = prices.CalcEma(emaWindow).ToArray();
        var bollingerBands = candles.CalcBollingerBands(bbWindow, stdDev);

        var length = candles.Length;
        var result = new IndicatorResult[length];

        for (int i = 1; i < length; i++)
        {
            result[i] = new IndicatorResult
            {
                Candle = candles[i],
                Signal = Signal.None, // Default to None
                Gain = (bollingerBands[i].UpperBand - bollingerBands[i].LowerBand) // Calculating Gain as the difference between the Bollinger Bands
            };

            if (candles[i].Spread <= maxSpread && candles[i].Volume >= minVolume)
            {
                if (ma16[i] > 0 && ma16[i - 1] <= 0) // MA crosses above zero
                {
                    if (oscillator[i] < 0 && oscillator[i - 1] >= 0) // Oscillator pullback below zero
                    {
                        // Check for sufficient gain and bollinger band conditions
                        if (emaResult[i] < bollingerBands[i].LowerBand && result[i].Gain >= minGain)
                        {
                            result[i].Signal = Signal.Buy;
                            result[i].TakeProfit = candles[i].CalcTakeProfit(result[i], riskReward);
                            result[i].StopLoss = candles[i].CalcStopLoss(result[i]);
                        }
                    }
                }
                else if (ma16[i] < 0 && ma16[i - 1] >= 0) // MA crosses below zero
                {
                    if (oscillator[i] > 0 && oscillator[i - 1] <= 0) // Oscillator pullback above zero
                    {
                        // Check for sufficient gain and bollinger band conditions
                        if (emaResult[i] > bollingerBands[i].UpperBand && result[i].Gain >= minGain)
                        {
                            result[i].Signal = Signal.Sell;
                            result[i].TakeProfit = candles[i].CalcTakeProfit(result[i], riskReward);
                            result[i].StopLoss = candles[i].CalcStopLoss(result[i]);
                        }
                    }
                }
            }
        }

        return result;
        return result;
    }



}
