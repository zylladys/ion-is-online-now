using ION.App.Pages;
using ION.Core.Models;
using ION.Core.Services;

namespace ION.App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();            
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        RefreshChannels();
    }

    private bool _testIsOnline;

    private void OnLogoStatusTapped(
    object? sender,
    TappedEventArgs e)
    {
        _testIsOnline = !_testIsOnline;

        UpdateGlobalStatus(_testIsOnline);
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

    private void RefreshChannels()
    {
        ChannelsContainer.Children.Clear();

        var channels = ChannelStore.Channels;

        if (channels.Count == 0)
        {
            ChannelsContainer.Children.Add(CreateEmptyState());
            return;
        }

        foreach (var channel in channels)
        {
            ChannelsContainer.Children.Add(
                CreateChannelCard(channel));
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

    private View CreateChannelCard(StreamChannel channel)
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
            Text = "○ NOT CHECKED",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#8F96A3")
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
            actions
        }
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

    private async Task RemoveChannelAsync(StreamChannel channel)
    {
        var confirmed = await DisplayAlertAsync(
            "Remove channel?",
            $"Stop monitoring {channel.DisplayName}?",
            "Remove",
            "Cancel");

        if (!confirmed)
            return;

        await ChannelStore.RemoveAsync(channel.Id);

        RefreshChannels();
    }

    private async void OnAddChannelClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(AddChannelPage));
    }
}