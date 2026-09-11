using ION.Core.Models;
using ION.Core.Platforms;
using ION.Server.Integrations.Twitch;

namespace ION.Server.Services;

public class StreamMonitorService : BackgroundService
{
    private readonly TwitchService _twitchService;
    private readonly ILogger<StreamMonitorService> _logger;

    private readonly LiveEventDispatcher _liveEventDispatcher;

    private readonly Dictionary<string, bool> _lastKnownStates =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly ServerChannelStore _channelStore;

    public StreamMonitorService(
    TwitchService twitchService,
    ServerChannelStore channelStore,
    LiveEventDispatcher liveEventDispatcher,
    ILogger<StreamMonitorService> logger)
    {
        _twitchService = twitchService;
        _channelStore = channelStore;
        _liveEventDispatcher = liveEventDispatcher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "ION Stream Monitor started.");

        // Establish the initial state without generating
        // a false OFFLINE -> LIVE event.
        await CheckChannelsAsync(
            detectTransitions: false,
            stoppingToken);

        using var timer =
            new PeriodicTimer(
                TimeSpan.FromSeconds(10));

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

        foreach (var status in results)
        {
            if (status is null)
                continue;

            if (detectTransitions &&
                _lastKnownStates.TryGetValue(
                    status.Username,
                    out var wasLive))
            {
                if (!wasLive && status.IsLive)
                {
                    var liveEvent =
                        CreateLiveEvent(status);

                    await OnChannelWentLiveAsync(liveEvent,cancellationToken);
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

            _lastKnownStates[status.Username] =
                status.IsLive;
        }
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

    private async Task<StreamStatus?> CheckChannelAsync(
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

            return status;
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