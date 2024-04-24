﻿using TradingApp.Models.DataTransferObjects;
using TradingApp.Models.Enums;

namespace TradingApp.Extensions.IndicatorExtensions;

public static partial class Indicator
{
    public static Signal[] CalcTrend(this Candle[] candles, int shortEma = 8, int longEma = 21)
    {
        var prices = candles.Select(c => c.Mid_C).ToArray();

        var shortEmaResult = prices.CalcEma(shortEma).ToArray();

        var longEmaResult = prices.CalcEma(longEma).ToArray();

        var length = candles.Length;

        var result = new Signal[length];

        for (var i = 0; i < length; i++)
        {
            if (shortEmaResult[i] > longEmaResult[i])
            {
                result[i] = Signal.Buy;
            }
            else if (shortEmaResult[i] < longEmaResult[i])
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