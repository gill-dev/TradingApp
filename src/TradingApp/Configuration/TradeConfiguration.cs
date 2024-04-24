﻿namespace TradingApp.Configuration;

public class TradeConfiguration
{
    public bool RunBot { get; set; }
    public bool StopRollover { get; set; }
    public int TradeRisk { get; set; }
    public TradeSettings[] TradeSettings { get; set; }
}