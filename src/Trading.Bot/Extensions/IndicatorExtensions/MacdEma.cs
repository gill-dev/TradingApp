using System;
using System.Linq;

namespace Trading.Bot.Extensions.IndicatorExtensions
{
    public static partial class Indicator
    {
        public static IndicatorResult[] CalcMacdEma(this Candle[] candles, int bbWindow = 20, int emaWindow = 100,
            double stdDev = 2, double maxSpread = 0.0004, double minGain = 0.0006, int minVolume = 100, double riskReward = 1.5, int atrPeriod = 14, double atrThreshold = 1)
        {
            var macd = candles.CalcMacd();
            var prices = candles.Select(c => c.Mid_C).ToArray();
            var ema = prices.CalcEma(emaWindow).ToArray();
            var atr = candles.CalcAtr(atrPeriod).Select(a => a.Atr).ToArray();
            
            var closePrices = candles.Select(c => c.Mid_C).ToArray();
            var highPrices = candles.Select(c => c.Mid_H).ToArray();
            var lowPrices = candles.Select(c => c.Mid_L).ToArray();

            var adx = closePrices.CalcAdx(highPrices, lowPrices, 14).ToArray();

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

                result[i].Gain = Math.Abs(candles[i].Mid_C - ema[i]);

                bool isTrending = adx[i].Adx > 25; // Consider market trending if ADX > 25
                bool isVolatile = atr[i] > atr.Skip(i - atrPeriod + 1).Take(atrPeriod).Average() * atrThreshold;

                // Adaptive parameters based on volatility
                double adaptiveMinGain = isVolatile ? minGain * 1.5 : minGain;
                double adaptiveMaxSpread = isVolatile ? maxSpread * 0.8 : maxSpread;

                result[i].Signal = direction switch
                {
                    1 when isTrending && candles[i].Mid_C > ema[i] &&
                        candles[i].Spread <= adaptiveMaxSpread &&
                        result[i].Gain >= adaptiveMinGain &&
                        candles[i].Volume >= minVolume &&
                        isVolatile => Signal.Buy,
                    -1 when isTrending && candles[i].Mid_C < ema[i] &&
                            candles[i].Spread <= adaptiveMaxSpread &&
                            result[i].Gain >= adaptiveMinGain &&
                            candles[i].Volume >= minVolume &&
                            isVolatile => Signal.Sell,
                    _ => Signal.None
                };

                // Dynamic risk management
                double atrMultiplier = isVolatile ? 2.0 : 1.5;
                result[i].StopLoss = result[i].Signal == Signal.Buy
                    ? candles[i].Mid_C - atr[i] * atrMultiplier
                    : candles[i].Mid_C + atr[i] * atrMultiplier;

                result[i].TakeProfit = result[i].Signal == Signal.Buy
                    ? candles[i].Mid_C + (candles[i].Mid_C - result[i].StopLoss) * riskReward
                    : candles[i].Mid_C - (result[i].StopLoss - candles[i].Mid_C) * riskReward;

                result[i].Loss = Math.Abs(candles[i].Mid_C - result[i].StopLoss);
            }

            return result;
        }
    }
}