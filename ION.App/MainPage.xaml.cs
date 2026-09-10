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

        var info = new VerticalStackLayout
        {
            Spacing = 3,

            Children =
            {
                platform,
                name,
                status
            }
        };

        return new Border
        {
            Padding = 16,
            BackgroundColor = Color.FromArgb("#17191E"),
            Stroke = Color.FromArgb("#2A2D34"),
            StrokeThickness = 1,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = 16
            },

            Content = info
        };
    }

    private async void OnAddChannelClicked(
        object? sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            nameof(AddChannelPage));
    }
}