using TradingApp.Models.DataTransferObjects;
using TradingApp.Models.Enums;

namespace TradingApp.Models.Indicators;

public abstract class IndicatorBase
{
    public Candle Candle { get; set; }
    public Signal Signal { get; set; }
    public double Gain { get; set; }
    public double TakeProfit { get; set; }
    public double StopLoss { get; set; }
    public double Loss { get; set; }
}