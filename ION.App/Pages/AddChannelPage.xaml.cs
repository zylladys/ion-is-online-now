using ION.Core.Models;
using ION.Core.Platforms;

namespace ION.App.Pages;

public partial class AddChannelPage : ContentPage
{
    private StreamingPlatform _detectedPlatform = StreamingPlatform.Unknown;

    public AddChannelPage()
    {
        InitializeComponent();
    }

    private void OnChannelTextChanged(object sender, TextChangedEventArgs e)
    {
        _detectedPlatform = PlatformDetector.Detect(e.NewTextValue);

        PlatformPanel.IsVisible = !string.IsNullOrWhiteSpace(e.NewTextValue);

        if (_detectedPlatform == StreamingPlatform.Unknown)
        {
            PlatformLabel.Text = "Unknown platform";
            PlatformIndicator.TextColor = Color.FromArgb("#8F96A3");
            AddButton.IsEnabled = false;
            return;
        }

        PlatformLabel.Text = _detectedPlatform.ToString();
        PlatformIndicator.TextColor = Color.FromArgb("#E9435B");
        AddButton.IsEnabled = true;
    }

    [Obsolete]
    private async void OnAddClicked(object sender, EventArgs e)
    {
        var channel = new StreamChannel
        {
            Input = ChannelEntry.Text?.Trim() ?? string.Empty,
            Platform = _detectedPlatform
        };

        await DisplayAlert(
            "Channel detected",
            $"Platform: {channel.Platform}\nInput: {channel.Input}",
            "OK");
    }
}