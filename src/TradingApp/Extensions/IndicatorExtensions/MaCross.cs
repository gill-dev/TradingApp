﻿using TradingApp.Extensions;
using TradingApp.Models.DataTransferObjects;
using TradingApp.Models.Enums;
using TradingApp.Models.Indicators;

namespace TradingApp.Extensions.IndicatorExtensions;

public static partial class Indicator
{
    public static MaCrossResult[] CalcMaCross(this Candle[] candles, int shortWindow = 10, int longWindow = 20,
        double maxSpread = 0.0004, double minGain = 0.0006, double riskReward = 1.5)
    {
        var typicalPrice = candles.Select(c => (c.Mid_C + c.Mid_H + c.Mid_L) / 3).ToArray();

        var maShort = typicalPrice.CalcSma(shortWindow).ToArray();

        var maLong = typicalPrice.CalcSma(longWindow).ToArray();

        var length = candles.Length;

        var result = new MaCrossResult[length];

        for (var i = 0; i < length; i++)
        {
            result[i] ??= new MaCrossResult();

            result[i].Candle = candles[i];

            result[i].MaShort = maShort[i];

            result[i].MaLong = maLong[i];

            result[i].Delta = maShort[i] - maLong[i];

            result[i].DeltaPrev = i > 0 ? result[i - 1].Delta : 0;

            result[i].Gain = Math.Abs(candles[i].Mid_C - result[i].MaShort);

            result[i].Signal = result[i].Delta switch
            {
                >= 0 when result[i].DeltaPrev < 0 &&
                          candles[i].Spread <= maxSpread &&
                          result[i].Gain >= minGain => Signal.Buy,
                < 0 when result[i].DeltaPrev >= 0 &&
                         candles[i].Spread <= maxSpread &&
                         result[i].Gain >= minGain => Signal.Sell,
                _ => Signal.None
            };

            result[i].TakeProfit = candles[i].CalcTakeProfit(result[i], riskReward);

            result[i].StopLoss = candles[i].CalcStopLoss(result[i]);

            result[i].Loss = Math.Abs(candles[i].Mid_C - result[i].StopLoss);
        }

        return result;
    }
}