namespace Trading.Bot.Extensions.IndicatorExtensions;

public static partial class Indicator
{
public static Signal[] CalcEmaTrend(this Candle[] candles, int fastEma = 50, int slowEma = 200)
{
    var prices = candles.Select(c => c.Mid_C).ToArray();
    var fastEmaResult = prices.CalcEma(fastEma).ToArray();
    var slowEmaResult = prices.CalcEma(slowEma).ToArray();
    var length = candles.Length;
    var result = new Signal[length];

    for (var i = 0; i < length; i++)
    {
        if (i < slowEma)
        {
            result[i] = Signal.None;
            continue;
        }

        // Check EMA cross trend
        bool isBullishTrend = fastEmaResult[i] > slowEmaResult[i];
        bool isStrongTrend = Math.Abs(fastEmaResult[i] - slowEmaResult[i]) > 
            (prices[i] * 0.0001);

        // Price position relative to EMAs
        bool priceAboveEmas = candles[i].Mid_L > Math.Max(fastEmaResult[i], slowEmaResult[i]);
        bool priceBelowEmas = candles[i].Mid_H < Math.Min(fastEmaResult[i], slowEmaResult[i]);

        if (isBullishTrend && isStrongTrend && priceAboveEmas)
        {
            result[i] = Signal.Buy;
        }
        else if (!isBullishTrend && isStrongTrend && priceBelowEmas)
        {
            result[i] = Signal.Sell;
        }
        else
        {
            result[i] = Signal.None;
        }
    }

    return result;
}
}