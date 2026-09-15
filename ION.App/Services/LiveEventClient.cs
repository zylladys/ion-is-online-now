using ION.Core.Models;

using Microsoft.AspNetCore.SignalR.Client;

using ION.App.Configuration;

namespace ION.App.Services;

public class LiveEventClient
{
    private readonly HubConnection _connection;

    public event Func<LiveEvent, Task>? LiveStarted;

    public event Func<OfflineEvent, Task>? LiveEnded;

    public LiveEventClient()
    {
        var serverAddress =
            IonServerConfiguration.BaseUrl;

        _connection =
            new HubConnectionBuilder()
                .WithUrl($"{serverAddress}/hubs/live")
                .WithAutomaticReconnect()
                .Build();

        _connection.On<LiveEvent>(
            "LiveStarted",
            async liveEvent =>
            {
                System.Diagnostics.Debug.WriteLine(
                    $"ION SignalR: LiveStarted received for {liveEvent.Username}");

                if (LiveStarted is not null)
                {
                    await LiveStarted.Invoke(liveEvent);
                }
            });

        _connection.On<OfflineEvent>(
    "LiveEnded",
    async offlineEvent =>
    {
        System.Diagnostics.Debug.WriteLine(
            $"ION SignalR: LiveEnded received for {offlineEvent.Username}");

        if (LiveEnded is not null)
        {
            await LiveEnded.Invoke(offlineEvent);
        }
    });

        _connection.Reconnecting += error =>
        {
            System.Diagnostics.Debug.WriteLine(
                $"ION SignalR reconnecting: {error?.Message}");

            return Task.CompletedTask;
        };

        _connection.Reconnected += connectionId =>
        {
            System.Diagnostics.Debug.WriteLine(
                $"ION SignalR reconnected: {connectionId}");

            return Task.CompletedTask;
        };

        _connection.Closed += error =>
        {
            System.Diagnostics.Debug.WriteLine(
                $"ION SignalR closed: {error?.Message}");

            return Task.CompletedTask;
        };
    }

    public async Task StartAsync()
    {
        if (_connection.State ==
            HubConnectionState.Disconnected)
        {
            await _connection.StartAsync();

            System.Diagnostics.Debug.WriteLine(
                "ION SignalR connected.");
        }
    }

    public async Task StopAsync()
    {
        if (_connection.State !=
            HubConnectionState.Disconnected)
        {
            await _connection.StopAsync();
        }
    }
}