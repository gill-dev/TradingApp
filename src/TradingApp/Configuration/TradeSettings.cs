﻿namespace TradingApp.Configuration;

public class TradeSettings
{
    public string Instrument { get; set; }
    public string MainGranularity { get; set; }
    public string LongerGranularity { get; set; }
    public TimeSpan CandleSpan { get; set; }
    public int[] Integers { get; set; }
    public double[] Doubles { get; set; }
    public double MaxSpread { get; set; }
    public double MinGain { get; set; }
    public int MinVolume { get; set; }
    public double RiskReward { get; set; }
}