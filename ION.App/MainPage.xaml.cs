using ION.App.Pages;
using ION.Core.Models;

using ION.App.Services;
using ION.Core.Platforms;

namespace ION.App;

public partial class MainPage : ContentPage
{
    private readonly IonApiClient _apiClient;

    private readonly INotificationService _notificationService;

    private readonly Dictionary<Guid, ChannelCardControls> _channelCards = [];

    private readonly Dictionary<Guid, bool> _currentLiveStates = [];

    private readonly LiveEventClient _liveEventClient;

    private List<StreamChannel> _channels = [];

    public MainPage(
    IonApiClient apiClient,
    INotificationService notificationService,
    LiveEventClient liveEventClient)
    {
        InitializeComponent();

        _apiClient = apiClient;
        _notificationService = notificationService;
        _liveEventClient = liveEventClient;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _liveEventClient.LiveStarted +=
            OnLiveStartedReceivedAsync;

        _liveEventClient.LiveEnded +=
            OnLiveEndedReceivedAsync;

        await _liveEventClient.StartAsync();

        _channels =
    (await _apiClient.GetChannelsAsync())
        .ToList();

        BuildChannelCards();

        await UpdateChannelStatusesAsync();
    }

    private void BuildChannelCards()
    {
        ChannelsContainer.Children.Clear();
        _channelCards.Clear();
        _currentLiveStates.Clear();

        var channels =
    _channels.ToList();

        if (channels.Count == 0)
        {
            ChannelsContainer.Children.Add(
                CreateEmptyState());

            UpdateGlobalStatus(false);

            return;
        }

        foreach (var channel in channels)
        {
            ChannelsContainer.Children.Add(
                CreateChannelCard(channel));
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _liveEventClient.LiveStarted -=
            OnLiveStartedReceivedAsync;

        _liveEventClient.LiveEnded -=
            OnLiveEndedReceivedAsync;
    }


    private void UpdateGlobalStatus(bool isOnline)
    {
        if (isOnline)
        {
            LogoStatusLabel.Text = "●";
            LogoStatusLabel.TextColor =
                Color.FromArgb("#E9435B");

            LogoStatusIndicator.BackgroundColor =
                Color.FromArgb("#20E9435B");

            GlobalStatusIndicator.Text = "●";
            GlobalStatusIndicator.TextColor =
                Color.FromArgb("#E9435B");

            GlobalStatusTitle.Text = "ONLINE";

            GlobalStatusDescription.Text =
                "A monitored channel is live right now";
        }
        else
        {
            LogoStatusLabel.Text = "○";
            LogoStatusLabel.TextColor =
                Color.FromArgb("#7D8490");

            LogoStatusIndicator.BackgroundColor =
                Colors.Transparent;

            GlobalStatusIndicator.Text = "○";
            GlobalStatusIndicator.TextColor =
                Color.FromArgb("#7D8490");

            GlobalStatusTitle.Text = "OFFLINE";

            GlobalStatusDescription.Text =
                "No channels are live right now";
        }
    }

    private View CreateEmptyState()
    {
        var title = new Label
        {
            Text = "No channels configured",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            HorizontalTextAlignment = TextAlignment.Center
        };

        var description = new Label
        {
            Text = "Add a livestream channel to start monitoring it.",
            FontSize = 13,
            TextColor = Color.FromArgb("#8F96A3"),
            HorizontalTextAlignment = TextAlignment.Center
        };

        return new Border
        {
            Padding = 24,
            BackgroundColor = Color.FromArgb("#17191E"),
            Stroke = Color.FromArgb("#2A2D34"),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = 16
            },

            Content = new VerticalStackLayout
            {
                Spacing = 8,
                HorizontalOptions = LayoutOptions.Center,

                Children =
                {
                    title,
                    description
                }
            }
        };
    }

    private View CreateChannelCard(
    StreamChannel channel)
    {
        var platform = new Label
        {
            Text = channel.Platform.ToString().ToUpperInvariant(),
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            CharacterSpacing = 1.5,
            TextColor = Color.FromArgb("#8F96A3")
        };

        var name = new Label
        {
            Text = channel.DisplayName,
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };

        var status = new Label
        {
            Text = "○ CHECKING...",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#8F96A3")
        };

        var details = new Label
        {
            Text = string.Empty,
            FontSize = 12,
            TextColor = Color.FromArgb("#8F96A3"),
            IsVisible = false
        };

        var openButton = new Button
        {
            Text = "Open Stream",
            HeightRequest = 42,
            CornerRadius = 12,
            BackgroundColor = Color.FromArgb("#252831"),
            TextColor = Colors.White,
            FontAttributes = FontAttributes.Bold
        };

        openButton.Clicked += async (_, _) =>
        {
            await OpenChannelAsync(channel);
        };

        var removeButton = new Button
        {
            Text = "Remove",
            HeightRequest = 42,
            CornerRadius = 12,
            BackgroundColor = Color.FromArgb("#351C22"),
            TextColor = Color.FromArgb("#E9435B"),
            FontAttributes = FontAttributes.Bold
        };

        removeButton.Clicked += async (_, _) =>
        {
            await RemoveChannelAsync(channel);
        };

        var actions = new Grid
        {
            ColumnSpacing = 10,
            Margin = new Thickness(0, 12, 0, 0),

            ColumnDefinitions =
        {
            new ColumnDefinition(GridLength.Star),
            new ColumnDefinition(GridLength.Star)
        }
        };

        actions.Add(openButton, 0, 0);
        actions.Add(removeButton, 1, 0);

        var content = new VerticalStackLayout
        {
            Spacing = 3,

            Children =
        {
            platform,
            name,
            status,
            details,
            actions
        }
        };

        _channelCards[channel.Id] =
    new ChannelCardControls
    {
        StatusLabel = status,
        DetailsLabel = details
    };

        return new Border
        {
            Padding = 16,
            BackgroundColor = Color.FromArgb("#17191E"),
            Stroke = Color.FromArgb("#2A2D34"),
            StrokeThickness = 1,

            StrokeShape =
                new Microsoft.Maui.Controls.Shapes.RoundRectangle
                {
                    CornerRadius = 16
                },

            Content = content
        };

    }

    private async Task UpdateChannelStatusesAsync()
    {
        var channels =
    _channels.ToList();

        var tasks =
            channels.Select(
                channel =>
                    GetChannelStatusAsync(channel));

        var results =
            await Task.WhenAll(tasks);

        var anyLive = false;

        foreach (var result in results)
        {
            var channel = result.Channel;
            var streamStatus = result.Status;

            if (streamStatus is null)
            {
                UpdateChannelCard(
                    channel,
                    null);

                continue;
            }

            _currentLiveStates[channel.Id] =
                streamStatus.IsLive;

            if (streamStatus.IsLive)
            {
                anyLive = true;
            }

            UpdateChannelCard(
                channel,
                streamStatus);
        }

        UpdateGlobalStatus(anyLive);
    }

    private async Task<(
    StreamChannel Channel,
    StreamStatus? Status)>
    GetChannelStatusAsync(
        StreamChannel channel)
    {
        StreamStatus? status = null;

        if (channel.Platform ==
            StreamingPlatform.Twitch)
        {
            status =
                await _apiClient.GetTwitchStatusAsync(
                    channel.Username);
        }

        return (
            channel,
            status);
    }

    private async Task OnLiveStartedReceivedAsync(
    LiveEvent liveEvent)
    {
        System.Diagnostics.Debug.WriteLine(
            $"ION APP RECEIVED LIVE EVENT: {liveEvent.Platform}/{liveEvent.Username}");

        var channel =
    _channels.FirstOrDefault(x =>
        x.Platform == liveEvent.Platform &&
        string.Equals(
            x.Username,
            liveEvent.Username,
            StringComparison.OrdinalIgnoreCase));

        if (channel is not null)
        {
            var status = new StreamStatus
            {
                Platform = liveEvent.Platform,
                Username = liveEvent.Username,
                DisplayName = liveEvent.DisplayName,
                IsLive = true,
                Title = liveEvent.Title,
                GameName = liveEvent.GameName,
                ViewerCount = liveEvent.ViewerCount,
                StartedAt = liveEvent.StartedAt
            };

            MainThread.BeginInvokeOnMainThread(() =>
            {
                _currentLiveStates[channel.Id] = true;

                UpdateChannelCard(
                    channel,
                    status);

                UpdateGlobalStatus(
                    _currentLiveStates.Values.Any(x => x));
            });
        }

        await _notificationService
            .ShowLiveNotificationAsync(
                liveEvent.DisplayName,
                liveEvent.GameName,
                liveEvent.Title,
                liveEvent.ChannelUrl);
    }

    private Task OnLiveEndedReceivedAsync(
    OfflineEvent offlineEvent)
    {
        System.Diagnostics.Debug.WriteLine(
            $"ION APP RECEIVED OFFLINE EVENT: {offlineEvent.Platform}/{offlineEvent.Username}");

        var channel =
    _channels.FirstOrDefault(x =>
        x.Platform == offlineEvent.Platform &&
        string.Equals(
            x.Username,
            offlineEvent.Username,
            StringComparison.OrdinalIgnoreCase));

        if (channel is not null)
        {
            var status = new StreamStatus
            {
                Platform = offlineEvent.Platform,
                Username = offlineEvent.Username,
                DisplayName = offlineEvent.DisplayName,
                IsLive = false
            };

            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateChannelCard(
                    channel,
                    status);

                _currentLiveStates[channel.Id] = false;

                UpdateGlobalStatus(
                    _currentLiveStates.Values.Any(x => x));
            });
        }

        return Task.CompletedTask;
    }

    private void UpdateChannelCard(
    StreamChannel channel,
    StreamStatus? status)
    {
        if (!_channelCards.TryGetValue(
            channel.Id,
            out var controls))
        {
            return;
        }

        if (status is null)
        {
            controls.StatusLabel.Text =
                "○ NOT CHECKED";

            controls.StatusLabel.TextColor =
                Color.FromArgb("#8F96A3");

            controls.DetailsLabel.IsVisible =
                false;

            return;
        }

        if (status.IsLive)
        {
            controls.StatusLabel.Text =
                "● LIVE";

            controls.StatusLabel.TextColor =
                Color.FromArgb("#E9435B");

            controls.DetailsLabel.Text =
                BuildLiveDetails(status);

            controls.DetailsLabel.IsVisible =
                true;
        }
        else
        {
            controls.StatusLabel.Text =
                "○ OFFLINE";

            controls.StatusLabel.TextColor =
                Color.FromArgb("#8F96A3");

            controls.DetailsLabel.Text =
                string.Empty;

            controls.DetailsLabel.IsVisible =
                false;
        }
    }



    private static string BuildLiveDetails(
    StreamStatus status)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(
            status.GameName))
        {
            parts.Add(status.GameName);
        }

        if (status.ViewerCount.HasValue)
        {
            parts.Add(
                $"{status.ViewerCount.Value} viewers");
        }

        return string.Join(
            " • ",
            parts);
    }

    private async Task OpenChannelAsync(StreamChannel channel)
    {
        if (string.IsNullOrWhiteSpace(channel.ChannelUrl))
            return;

        try
        {
            var uri = new Uri(channel.ChannelUrl);

            var opened = await Browser.Default.OpenAsync(
                uri,
                BrowserLaunchMode.SystemPreferred);

            if (!opened)
            {
                await DisplayAlertAsync(
                    "Could not open stream",
                    "ION could not open this channel.",
                    "OK");
            }
        }
        catch
        {
            await DisplayAlertAsync(
                "Could not open stream",
                "ION could not open this channel.",
                "OK");
        }
    }

    private async Task RemoveChannelAsync(
    StreamChannel channel)
    {
        var confirmed = await DisplayAlertAsync(
            "Remove channel?",
            $"Stop monitoring {channel.DisplayName}?",
            "Remove",
            "Cancel");

        if (!confirmed)
            return;

        var removed =
            await _apiClient.RemoveChannelAsync(
                channel.Id);

        if (!removed)
        {
            await DisplayAlertAsync(
                "Could not remove channel",
                "ION could not remove this channel from the server.",
                "OK");

            return;
        }

        _channels.RemoveAll(
    x => x.Id == channel.Id);

        BuildChannelCards();

        await UpdateChannelStatusesAsync();

        BuildChannelCards();

        await UpdateChannelStatusesAsync();
    }

    private async void OnAddChannelClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(AddChannelPage));
    }

    private sealed class ChannelCardControls
    {
        public required Label StatusLabel { get; init; }

        public required Label DetailsLabel { get; init; }
    }

}