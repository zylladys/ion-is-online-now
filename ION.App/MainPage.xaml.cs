namespace ION.App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    [Obsolete]
    private async void OnAddChannelClicked(object sender, EventArgs e)
    {
        await DisplayAlert(
            "ION",
            "Channel configuration is coming next.",
            "OK");
    }
}