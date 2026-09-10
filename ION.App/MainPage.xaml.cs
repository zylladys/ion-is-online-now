using ION.App.Pages;

namespace ION.App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnAddChannelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddChannelPage));
    }
}