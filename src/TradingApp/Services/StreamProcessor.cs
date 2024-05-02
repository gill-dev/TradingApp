﻿using Trading.Bot.Extensions;
using TradingApp.Configuration;
using TradingApp.Extensions;
using TradingApp.Models.DataTransferObjects;

namespace TradingApp.Services;

public class StreamProcessor : BackgroundService
{
    private readonly ILogger<StreamProcessor> _logger;
    private readonly LiveTradeCache _liveTradeCache;
    private readonly TradeConfiguration _tradeConfiguration;
    private readonly List<string> _instruments = new();
    private readonly Dictionary<string, DateTime> _lastCandleTimings = new();
    private readonly ParallelOptions _options = new();

    public StreamProcessor(ILogger<StreamProcessor> logger, LiveTradeCache liveTradeCache, TradeConfiguration tradeConfiguration)
    {
        _logger = logger;
        _liveTradeCache = liveTradeCache;
        _tradeConfiguration = tradeConfiguration;
        _options.MaxDegreeOfParallelism = _tradeConfiguration.TradeSettings.Length / 2;

        foreach (var tradeSetting in _tradeConfiguration.TradeSettings)
        {
            _instruments.Add(tradeSetting.Instrument);

            _lastCandleTimings[tradeSetting.Instrument] = DateTime.UtcNow.RoundDown(tradeSetting.CandleSpan);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Parallel.ForEachAsync(_instruments, _options, async (instrument, token) =>
            {
                try
                {
                    if (_liveTradeCache.LivePrices.ContainsKey(instrument))
                    {
                        await DetectNewCandle(_liveTradeCache.LivePrices[instrument], token);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred when trying to detect a new candle");
                }
            });

            await Task.Delay(10, stoppingToken);
        }
    }

    private async Task DetectNewCandle(LivePrice livePrice, CancellationToken stoppingToken)
    {
        var candleSpan = _tradeConfiguration.TradeSettings.First(x =>
            x.Instrument == livePrice.Instrument).CandleSpan;

        var current = livePrice.Time.RoundDown(candleSpan);

        if (current <= _lastCandleTimings[livePrice.Instrument]) return;

        _lastCandleTimings[livePrice.Instrument] = current;

        livePrice.Time = current;

        await _liveTradeCache.LivePriceChannel.Writer.WriteAsync(livePrice, stoppingToken);
    }
}