using TradingApp.Models.DataTransferObjects;
using TradingApp.Models.Enums;
using TradingApp.Models.Indicators;

namespace TradingApp.Extensions;

public static class NumericExtensions
{
    public static IEnumerable<double> CalcCma(this double[] sequence)
    {
        if (sequence is null)
        {
            yield break;
        }

        double total = 0;

        var count = 0;

        var length = sequence.Length;

        for (var i = 0; i < length; i++)
        {
            count++;

            total += sequence[i];

            yield return total / count;
        }
    }

    public static IEnumerable<double> CalcSma(this double[] sequence, int window)
    {
        if (sequence is null)
        {
            yield break;
        }

        var length = sequence.Length;

        if (length <= window)
        {
            yield return 0.0;
        }

        var queue = new Queue<double>(window);

        for (var i = 0; i < length; i++)
        {
            if (queue.Count == window)
            {
                queue.Dequeue();
            }

            queue.Enqueue(sequence[i]);

            yield return queue.Average();
        }
    }

    public static IEnumerable<double> CalcEma(this double[] sequence, int window)
    {
        if (sequence is null)
        {
            yield break;
        }

        var length = sequence.Length;

        if (length <= window)
        {
            var average = sequence.Take(length).Average();
            for (int i = 0; i < length; i++)
            {
                yield return average;
            }
        }

        var alpha = 2.0 / (window + 1);

        var result = 0.0;

        for (var i = 0; i < length; i++)
        {
            result = i == 0
                ? sequence[i]
                : alpha * sequence[i] + (1 - alpha) * result;

            yield return result;
        }
    }

    public static IEnumerable<double> CalcRma(this double[] sequence, int window)
    {
        if (sequence is null)
        {
            yield break;
        }

        var length = sequence.Length;

        if (length <= window)
        {
            yield return 0.0;
        }

        var alpha = 1.0 / window;

        var result = 0.0;

        for (var i = 0; i < length; i++)
        {
            result = i == 0
                ? sequence[i]
                : alpha * sequence[i] + (1 - alpha) * result;

            yield return result;
        }
    }

    public static IEnumerable<double> CalcRolStdDev(this double[] sequence, int window, double std)
    {
        if (sequence is null)
        {
            yield break;
        }

        var length = sequence.Length;

        if (length <= window)
        {
            yield return 0.0;
        }

        var queue = new Queue<double>(window);

        for (var i = 0; i < length; i++)
        {
            if (queue.Count == window)
            {
                queue.Dequeue();
            }

            queue.Enqueue(sequence[i]);

            yield return queue.CalcStdDev(std);
        }
    }

    public static double CalcStdDev(this IEnumerable<double> sequence, double std)
    {
        if (sequence is null)
        {
            return 0.0;
        }

        var list = sequence.ToArray();

        var length = list.Length;

        if (length <= 1)
        {
            return 0.0;
        }

        var average = list.Average();

        double sum = 0;

        for (var i = 0; i < length; i++)
        {
            sum += Math.Pow(Math.Abs(list[i] - average), std);
        }

        return Math.Sqrt(sum / length).NaN2Zero();
    }

    public static double NaN2Zero(this double value)
        => double.IsNaN(value)
            ? 0.0
            : value;

    public static List<double> FindResistanceLevels(Candle[] candles, int lookbackPeriod)
    {
        List<double> resistanceLevels = new List<double>();

        for (int i = lookbackPeriod; i < candles.Length; i++)
        {
            double high = candles[i].Mid_H;
            bool isResistance = true;
            for (int j = 1; j <= lookbackPeriod; j++)
            {
                if (candles[i - j].Mid_H >= high || candles[i + j].Mid_H >= high)
                {
                    isResistance = false;
                    break;
                }
            }
            if (isResistance)
                resistanceLevels.Add(high);
        }

        return resistanceLevels.Distinct().ToList();
    }

    public static bool CheckIfSupport(Candle[] candles, double resistanceLevel, double tolerance)
    {
        bool broken = false;
        foreach (var candle in candles)
        {
            if (candle.Mid_C > resistanceLevel)
            {
                broken = true; // The level was broken
                break;
            }
        }

        if (!broken) return false; // If never broken, it can't turn into support

        // Check for retest as support
        foreach (var candle in candles)
        {
            if (broken && Math.Abs(candle.Mid_L - resistanceLevel) <= tolerance)
            {
                if (candle.Mid_C > candle.Mid_L) // Confirm the price bounced off the level
                    return true;
            }
        }

        return false;
    }
    public static double CalcStopLoss(this Candle candle, IndicatorResult result, double riskReward, double atr)
    {
        double stopLoss = result.Signal == Signal.Buy
            ? candle.Mid_C - atr * riskReward // for Buy signal
            : candle.Mid_C + atr * riskReward; // for Sell signal

        return stopLoss;
    }
    public static IEnumerable<double> CalcAtr(this Candle[] candles, int atrWindow = 14)
    {
        double prevClose = candles[0].Mid_C;
        double atr = 0;

        for (int i = 1; i < candles.Length; i++)
        {
            double highLow = candles[i].Mid_H - candles[i].Mid_L;
            double highClose = Math.Abs(candles[i].Mid_H - prevClose);
            double lowClose = Math.Abs(candles[i].Mid_L - prevClose);

            double tr = Math.Max(highLow, Math.Max(highClose, lowClose));
            atr = (atr * (atrWindow - 1) + tr) / atrWindow;

            yield return atr;

            prevClose = candles[i].Mid_C;
        }
    }

}