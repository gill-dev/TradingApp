namespace Trading.Bot.Extensions.IndicatorExtensions;

public static partial class Indicator
{
     public static bool IsTrendStrong(this Candle[] candles, Signal signal, int emaWindow = 200)
    {
        var prices = candles.Select(c => c.Mid_C).ToArray();
        var ema = prices.CalcEma(emaWindow).ToArray();
        
        // Check last several candles for trend consistency
        int checkPeriod = Math.Min(10, candles.Length);
        var recentCandles = candles.Skip(candles.Length - checkPeriod).Take(checkPeriod);
        
        if (signal == Signal.Buy)
        {
            return recentCandles.All(c => c.Mid_C > ema[candles.Length - checkPeriod]);
        }
        else if (signal == Signal.Sell)
        {
            return recentCandles.All(c => c.Mid_C < ema[candles.Length - checkPeriod]);
        }
        
        return false;
    }
}
