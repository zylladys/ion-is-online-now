using ION.Core.Models;
using ION.Server.Hubs;

using Microsoft.AspNetCore.SignalR;

namespace ION.Server.Services;

public class LiveEventDispatcher
{
    private readonly IHubContext<LiveHub> _liveHub;
    private readonly ILogger<LiveEventDispatcher> _logger;

    public LiveEventDispatcher(
        IHubContext<LiveHub> liveHub,
        ILogger<LiveEventDispatcher> logger)
    {
        _liveHub = liveHub;
        _logger = logger;
    }

    public async Task DispatchAsync(
        LiveEvent liveEvent,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Dispatching LIVE event for {Platform}/{Username}.",
            liveEvent.Platform,
            liveEvent.Username);

        await _liveHub.Clients.All.SendAsync(
            "LiveStarted",
            liveEvent,
            cancellationToken);

        _logger.LogInformation(
            "LIVE event sent through SignalR for {Platform}/{Username}.",
            liveEvent.Platform,
            liveEvent.Username);
    }

    public async Task DispatchOfflineAsync(
    OfflineEvent offlineEvent,
    CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Dispatching OFFLINE event for {Platform}/{Username}.",
            offlineEvent.Platform,
            offlineEvent.Username);

        await _liveHub.Clients.All.SendAsync(
            "LiveEnded",
            offlineEvent,
            cancellationToken);

        _logger.LogInformation(
            "OFFLINE event sent through SignalR for {Platform}/{Username}.",
            offlineEvent.Platform,
            offlineEvent.Username);
    }
}