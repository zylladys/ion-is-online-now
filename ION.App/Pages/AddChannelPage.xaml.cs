using ION.App.Services;
using ION.Core.Platforms;

namespace ION.App.Pages;

public partial class AddChannelPage : ContentPage
{
    private readonly IonApiClient _apiClient;

    private StreamingPlatform _detectedPlatform =
        StreamingPlatform.Unknown;

    public AddChannelPage(
        IonApiClient apiClient)
    {
        InitializeComponent();

        _apiClient = apiClient;
    }

    private void OnChannelTextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        _detectedPlatform =
            PlatformDetector.Detect(e.NewTextValue);

        PlatformPanel.IsVisible =
            !string.IsNullOrWhiteSpace(e.NewTextValue);

        if (_detectedPlatform == StreamingPlatform.Unknown)
        {
            PlatformLabel.Text = "Unknown platform";
            PlatformIndicator.TextColor =
                Color.FromArgb("#8F96A3");

            AddButton.IsEnabled = false;
            return;
        }

        PlatformLabel.Text =
            _detectedPlatform.ToString();

        PlatformIndicator.TextColor =
            Color.FromArgb("#E9435B");

        AddButton.IsEnabled = true;
    }

    private async void OnAddClicked(
        object? sender,
        EventArgs e)
    {
        var channel =
            ChannelInputParser.Parse(ChannelEntry.Text ?? "");

        if (channel is null)
        {
            await DisplayAlertAsync(
                "Invalid channel",
                "ION could not understand this channel address.",
                "OK");

            return;
        }

        var added =
    await _apiClient.AddChannelAsync(channel);

        if (!added)
        {
            await DisplayAlertAsync(
                "Could not add channel",
                "ION could not add this channel. It may already exist or the server may be unavailable.",
                "OK");

            return;
        }

        await Shell.Current.GoToAsync("..");
    }
}