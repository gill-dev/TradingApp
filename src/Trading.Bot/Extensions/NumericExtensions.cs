namespace Trading.Bot.Extensions;

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
            yield return 0.0;
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
public static IEnumerable<AdxResult> CalcAdx(this double[] closePrices, double[] highPrices, double[] lowPrices, int period = 14)
    {
        if (closePrices == null || highPrices == null || lowPrices == null || 
            closePrices.Length != highPrices.Length || closePrices.Length != lowPrices.Length)
        {
            yield break;
        }

        var length = closePrices.Length;

        if (length <= period)
        {
            yield return new AdxResult();
        }

        var trueRange = new double[length];
        var directionalMovementPlus = new double[length];
        var directionalMovementMinus = new double[length];

        for (var i = 1; i < length; i++)
        {
            var highDiff = highPrices[i] - highPrices[i - 1];
            var lowDiff = lowPrices[i - 1] - lowPrices[i];

            trueRange[i] = Math.Max(highPrices[i] - lowPrices[i],
                Math.Max(Math.Abs(highPrices[i] - closePrices[i - 1]),
                Math.Abs(lowPrices[i] - closePrices[i - 1])));

            directionalMovementPlus[i] = highDiff > lowDiff && highDiff > 0 ? highDiff : 0;
            directionalMovementMinus[i] = lowDiff > highDiff && lowDiff > 0 ? lowDiff : 0;
        }

        var smoothedTrueRange = trueRange.Skip(1).Take(period).Sum();
        var smoothedDirectionalMovementPlus = directionalMovementPlus.Skip(1).Take(period).Sum();
        var smoothedDirectionalMovementMinus = directionalMovementMinus.Skip(1).Take(period).Sum();

        double prevAdx = 0;

        for (var i = period; i < length; i++)
        {
            smoothedTrueRange = smoothedTrueRange - (smoothedTrueRange / period) + trueRange[i];
            smoothedDirectionalMovementPlus = smoothedDirectionalMovementPlus - (smoothedDirectionalMovementPlus / period) + directionalMovementPlus[i];
            smoothedDirectionalMovementMinus = smoothedDirectionalMovementMinus - (smoothedDirectionalMovementMinus / period) + directionalMovementMinus[i];

            var diPlus = (smoothedDirectionalMovementPlus / smoothedTrueRange) * 100;
            var diMinus = (smoothedDirectionalMovementMinus / smoothedTrueRange) * 100;

            var dx = Math.Abs(diPlus - diMinus) / (diPlus + diMinus) * 100;

            var adx = i == period ? dx : (prevAdx * (period - 1) + dx) / period;
            prevAdx = adx;

            yield return new AdxResult
            {
                DiPlus = diPlus,
                DiMinus = diMinus,
                Dx = dx,
                Adx = adx
            };
        }
    }
    public static IEnumerable<double> CalcTema(this double[] sequence, int window)
    {
        var ema1 = sequence.CalcEma(window).ToArray();

        var ema2 = ema1.CalcEma(window).ToArray();

        var ema3 = ema2.CalcEma(window).ToArray();

        var length = sequence.Length;

        var tema = new double[length];

        for (var i = 0; i < length; i++)
        {
            tema[i] = 3.0 * ema1[i] - 3 * ema2[i] + ema3[i];
        }

        return tema;
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

    public static IEnumerable<double> CalcRolStdDev(this double[] sequence, int window)
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

            yield return queue.CalcStdDev();
        }
    }

    public static double CalcStdDev(this IEnumerable<double> sequence)
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

        double sumSq = 0;

        for (var i = 0; i < length; i++)
        {
            var value = list[i];
            sumSq += (value - average) * (value - average);
        }

        return Math.Sqrt(sumSq / length).NaN2Zero();
    }

    public static double NaN2Zero(this double value)
        => double.IsNaN(value)
            ? 0.0
            : value;
}