using TradingApp.Models.DataTransferObjects;
using TradingApp.Models.Enums;
using TradingApp.Models.Indicators;

namespace TradingApp.Extensions.IndicatorExtensions
{
    public static class TrailingStopLoss
    {
        public static double CalcTrailingStopLoss(this Candle candle, IndicatorResult indicator, double trailStopPercentage)
        {
            double trailingStopPrice;

            // Check the direction of the trade
            if (indicator.Signal == Signal.Buy)
            {
                // For a Buy order, the stop loss moves up, staying a fixed percentage below the highest price reached
                double highestPriceSinceOpen =
                    indicator.Gain;
                trailingStopPrice = highestPriceSinceOpen * (1 - trailStopPercentage / 100);
            }
            else if (indicator.Signal == Signal.Sell)
            {
                // For a Sell order, the stop loss moves down, staying a fixed percentage above the lowest price reached
                double lowestPriceSinceOpen =
                    indicator.Loss;
                trailingStopPrice = lowestPriceSinceOpen * (1 + trailStopPercentage / 100);
            }
            else
            {
                // If no trade is open, you can return the current stop loss
                trailingStopPrice = indicator.StopLoss;
            }

            return trailingStopPrice;
        }
    }
}
