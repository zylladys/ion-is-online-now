using ION.App.Pages;

namespace ION.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(
            nameof(AddChannelPage),
            typeof(AddChannelPage));
    }
}