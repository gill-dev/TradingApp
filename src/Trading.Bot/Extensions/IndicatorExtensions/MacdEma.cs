namespace Trading.Bot.Extensions.IndicatorExtensions
{
    public static partial class Indicator
    {
        public static IndicatorResult[] CalcMacdEma(this Candle[] candles, int bbWindow = 20, int emaWindow = 100,
            double stdDev = 2, double maxSpread = 0.0004, double minGain = 0.0006, int minVolume = 100, double riskReward = 1.5)
        {
            var macd = candles.CalcMacd().ToArray();
            var prices = candles.Select(c => c.Mid_C).ToArray();

            var emaShort = prices.CalcEma(15).ToArray();
            var emaLong = prices.CalcEma(60).ToArray();

            var length = candles.Length;
            var result = new IndicatorResult[length];

            for (var i = 0; i < length; i++)
            {
                result[i] ??= new IndicatorResult();
                result[i].Candle = candles[i];

                var macDelta = macd[i].Macd - macd[i].SignalLine;
                var macDeltaPrev = i == 0 ? 0.0 : macd[i - 1].Macd - macd[i - 1].SignalLine;

                // Determine direction based on MACD line crossing
                var direction = macDelta switch
                {
                    > 0 when macDeltaPrev < 0 => 1,
                    < 0 when macDeltaPrev > 0 => -1,
                    _ => 0
                };

                result[i].Gain = Math.Abs(candles[i].Mid_C - emaShort[i]);

                // Check criteria for long trades
                if (emaShort[i] > emaLong[i] && macd[i].Macd < 0)
                {
                    result[i].Signal = direction == 1 && candles[i].Spread <= maxSpread && result[i].Gain >= minGain
                        ? Signal.Buy
                        : Signal.None;
                }
                // Check criteria for short trades
                else if (emaShort[i] < emaLong[i] && macd[i].Macd > 0 )
                {
                    result[i].Signal = direction == -1 && candles[i].Spread <= maxSpread && result[i].Gain >= minGain
                        ? Signal.Sell
                        : Signal.None;
                }
                else
                {
                    result[i].Signal = Signal.None;
                }

                result[i].TakeProfit = candles[i].CalcTakeProfit(result[i], riskReward);
                result[i].StopLoss = candles[i].CalcStopLoss(result[i]);
                result[i].Loss = Math.Abs(candles[i].Mid_C - result[i].StopLoss);
            }

            return result;
        }

    }
}
