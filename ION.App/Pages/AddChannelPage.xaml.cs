using ION.Core.Platforms;
using ION.Core.Services;

namespace ION.App.Pages;

public partial class AddChannelPage : ContentPage
{
    private StreamingPlatform _detectedPlatform =
        StreamingPlatform.Unknown;

    public AddChannelPage()
    {
        InitializeComponent();
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

        await ChannelStore.AddAsync(channel);

        await Shell.Current.GoToAsync("..");
    }
}