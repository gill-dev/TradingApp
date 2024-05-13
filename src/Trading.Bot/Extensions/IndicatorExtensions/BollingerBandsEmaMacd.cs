namespace Trading.Bot.Extensions.IndicatorExtensions;

public static partial class Indicator
{
    public static IndicatorResult[] CalcBollingerBandsEmaMacd(this Candle[] candles, int bbWindow = 20, int emaWindow = 100,
        double stdDev = 2, double maxSpread = 0.0004, double minGain = 0.0006, int minVolume = 100, double riskReward = 1.5)
    {
        var prices = candles.Select(c => c.Mid_C).ToArray();

        var emaShortResult = prices.CalcEma(5).ToArray();
        var emaLongResult = prices.CalcEma(20).ToArray();

        var bollingerBands = candles.CalcBollingerBands(bbWindow, stdDev);

        var atr = candles.CalcAtr(14);

        var length = candles.Length;

        var result = new IndicatorResult[length];

        for (var i = 1; i < length; i++)
        {
            result[i] ??= new IndicatorResult();

            result[i].Candle = candles[i];

            double bbWidth = bollingerBands[i].UpperBand - bollingerBands[i].LowerBand;
            result[i].Gain = bbWidth;

            bool isBuySignal = (emaShortResult[i] > emaLongResult[i] && emaShortResult[i - 1] <= emaLongResult[i - 1]) &&
                               (prices[i] <= bollingerBands[i].LowerBand || (prices[i] > bollingerBands[i].Sma && prices[i - 1] <= bollingerBands[i].Sma)) &&
                               (candles[i].Spread <= maxSpread && candles[i].Volume >= minVolume && result[i].Gain >= minGain);

            bool isSellSignal = (emaShortResult[i] < emaLongResult[i] && emaShortResult[i - 1] >= emaLongResult[i - 1]) &&
                                (prices[i] >= bollingerBands[i].UpperBand || (prices[i] < bollingerBands[i].Sma && prices[i - 1] >= bollingerBands[i].Sma)) &&
                                (candles[i].Spread <= maxSpread && candles[i].Volume >= minVolume && result[i].Gain >= minGain);

            result[i].Signal = isBuySignal ? Signal.Buy :
                isSellSignal ? Signal.Sell : Signal.None;

            result[i].TakeProfit = candles[i].CalcTakeProfit(result[i], riskReward);
            result[i].StopLoss = candles[i].CalcStopLoss(result[i]);
            result[i].Loss = Math.Abs(candles[i].Mid_C - result[i].StopLoss);
        }

        return result;
    }
}