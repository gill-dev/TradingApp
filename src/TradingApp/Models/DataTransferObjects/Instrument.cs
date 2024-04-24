﻿using TradingApp.Models.ApiResponses;

namespace TradingApp.Models.DataTransferObjects;

public class Instrument
{
    public string Name { get; }
    public string Type { get; }
    public string DisplayName { get; }
    public double PipLocation { get; }
    public int DisplayPrecision { get; }
    public int TradeUnitsPrecision { get; }
    public double MarginRate { get; }

    public Instrument(InstrumentResponse instrument)
    {
        Name = instrument.Name;
        Type = instrument.Type;
        DisplayName = instrument.DisplayName;
        PipLocation = Math.Pow(10, instrument.PipLocation);
        DisplayPrecision = instrument.DisplayPrecision;
        TradeUnitsPrecision = instrument.TradeUnitsPrecision;
        MarginRate = instrument.MarginRate;
    }
}