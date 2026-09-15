using ION.Core.Models;
using ION.Core.Platforms;
using ION.Server.Integrations.Twitch;

namespace ION.Server.Services;

public class StreamMonitorService : BackgroundService
{
    private readonly TwitchService _twitchService;
    private readonly ServerChannelStore _channelStore;
    private readonly LiveEventDispatcher _liveEventDispatcher;
    private readonly ILogger<StreamMonitorService> _logger;

    private readonly int _intervalSeconds;

    private readonly Dictionary<Guid, bool> _lastKnownStates = [];

    private sealed record ChannelCheckResult(
        Guid ChannelId,
        StreamStatus Status);

    public StreamMonitorService(
        TwitchService twitchService,
        ServerChannelStore channelStore,
        LiveEventDispatcher liveEventDispatcher,
        IConfiguration configuration,
        ILogger<StreamMonitorService> logger)
    {
        _twitchService = twitchService;
        _channelStore = channelStore;
        _liveEventDispatcher = liveEventDispatcher;
        _logger = logger;

        _intervalSeconds =
            configuration.GetValue<int?>(
                "Monitoring:IntervalSeconds")
            ?? 60;

        if (_intervalSeconds <= 0)
            _intervalSeconds = 60;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ION Stream Monitor started. Polling interval: {IntervalSeconds}s.",
            _intervalSeconds);

        // Establish the initial state without generating
        // a false OFFLINE -> LIVE event.
        await CheckChannelsAsync(
            detectTransitions: false,
            stoppingToken);

        using var timer =
            new PeriodicTimer(
                TimeSpan.FromSeconds(_intervalSeconds));

        try
        {
            while (await timer.WaitForNextTickAsync(
                stoppingToken))
            {
                await CheckChannelsAsync(
                    detectTransitions: true,
                    stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the server shuts down.
        }

        _logger.LogInformation(
            "ION Stream Monitor stopped.");
    }

    private async Task CheckChannelsAsync(
        bool detectTransitions,
        CancellationToken cancellationToken)
    {
        var channels =
            await _channelStore.GetAllAsync();

        var twitchChannels =
            channels
                .Where(x =>
                    x.Platform == StreamingPlatform.Twitch)
                .ToList();

        var tasks =
            twitchChannels.Select(
                channel =>
                    CheckChannelAsync(
                        channel,
                        cancellationToken));

        var results =
            await Task.WhenAll(tasks);

        foreach (var result in results)
        {
            if (result is null)
                continue;

            var status = result.Status;
            var channelId = result.ChannelId;

            if (detectTransitions &&
                _lastKnownStates.TryGetValue(
                    channelId,
                    out var wasLive))
            {
                if (!wasLive && status.IsLive)
                {
                    var liveEvent =
                        CreateLiveEvent(status);

                    await OnChannelWentLiveAsync(
                        liveEvent,
                        cancellationToken);
                }

                if (wasLive && !status.IsLive)
                {
                    var offlineEvent =
                        CreateOfflineEvent(status);

                    await OnChannelWentOfflineAsync(
                        offlineEvent,
                        cancellationToken);
                }
            }

            _lastKnownStates[channelId] =
                status.IsLive;
        }

        RemoveStaleChannelStates(channels);
    }

    private void RemoveStaleChannelStates(
        IReadOnlyCollection<StreamChannel> channels)
    {
        var activeChannelIds =
            channels
                .Select(x => x.Id)
                .ToHashSet();

        var staleChannelIds =
            _lastKnownStates.Keys
                .Where(id =>
                    !activeChannelIds.Contains(id))
                .ToList();

        foreach (var channelId in staleChannelIds)
            _lastKnownStates.Remove(channelId);
    }

    private OfflineEvent CreateOfflineEvent(
        StreamStatus status)
    {
        return new OfflineEvent
        {
            Platform =
                status.Platform,

            Username =
                status.Username,

            DisplayName =
                status.DisplayName
                ?? status.Username,

            ChannelUrl =
                BuildChannelUrl(status)
        };
    }

    private async Task OnChannelWentOfflineAsync(
        OfflineEvent offlineEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "⚫ OFFLINE EVENT: {Platform}/{Username}",
            offlineEvent.Platform,
            offlineEvent.Username);

        await _liveEventDispatcher.DispatchOfflineAsync(
            offlineEvent,
            cancellationToken);
    }

    private LiveEvent CreateLiveEvent(
        StreamStatus status)
    {
        return new LiveEvent
        {
            Platform =
                status.Platform,

            Username =
                status.Username,

            DisplayName =
                status.DisplayName
                ?? status.Username,

            ChannelUrl =
                BuildChannelUrl(status),

            Title =
                status.Title,

            GameName =
                status.GameName,

            ViewerCount =
                status.ViewerCount,

            StartedAt =
                status.StartedAt
        };
    }

    private async Task OnChannelWentLiveAsync(
        LiveEvent liveEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            """
            🔴 LIVE EVENT
            Platform: {Platform}
            Channel: {DisplayName}
            Username: {Username}
            Game: {GameName}
            Title: {Title}
            Viewers: {ViewerCount}
            URL: {ChannelUrl}
            StartedAt: {StartedAt}
            DetectedAt: {DetectedAt}
            """,
            liveEvent.Platform,
            liveEvent.DisplayName,
            liveEvent.Username,
            liveEvent.GameName,
            liveEvent.Title,
            liveEvent.ViewerCount,
            liveEvent.ChannelUrl,
            liveEvent.StartedAt,
            liveEvent.DetectedAt);

        await _liveEventDispatcher.DispatchAsync(
            liveEvent,
            cancellationToken);
    }

    private static string BuildChannelUrl(
        StreamStatus status)
    {
        return status.Platform switch
        {
            StreamingPlatform.Twitch =>
                $"https://twitch.tv/{status.Username}",

            StreamingPlatform.YouTube =>
                $"https://youtube.com/@{status.Username}",

            StreamingPlatform.Kick =>
                $"https://kick.com/{status.Username}",

            StreamingPlatform.Picarto =>
                $"https://picarto.tv/{status.Username}",

            StreamingPlatform.Piczel =>
                $"https://piczel.tv/watch/{status.Username}",

            StreamingPlatform.TikTok =>
                $"https://tiktok.com/@{status.Username}",

            StreamingPlatform.Instagram =>
                $"https://instagram.com/{status.Username}",

            _ =>
                string.Empty
        };
    }

    private async Task<ChannelCheckResult?> CheckChannelAsync(
        StreamChannel channel,
        CancellationToken cancellationToken)
    {
        try
        {
            var status =
                await _twitchService.GetStreamStatusAsync(
                    channel.Username);

            if (status is null)
                return null;

            await _channelStore.UpdateLiveStateAsync(
                channel.Id,
                status.IsLive);

            _logger.LogDebug(
                "Checked {Username}: {State}",
                channel.Username,
                status.IsLive ? "LIVE" : "OFFLINE");

            return new ChannelCheckResult(
                channel.Id,
                status);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to check Twitch channel {Username}.",
                channel.Username);

            return null;
        }
    }
}