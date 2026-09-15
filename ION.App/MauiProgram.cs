using Microsoft.Extensions.Logging;

using ION.App.Services;

using ION.App.Configuration;

namespace ION.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        var builder = MauiApp.CreateBuilder();


#if WINDOWS
builder.Services.AddSingleton<
    INotificationService,
    ION.App.Platforms.Windows.WindowsNotificationService>();
#elif ANDROID
        builder.Services.AddSingleton<
            INotificationService,
            AndroidNotificationService>();
#endif

        builder
            .UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        builder.Services.AddHttpClient<IonApiClient>(client =>
        {
            client.BaseAddress =
                new Uri(IonServerConfiguration.BaseUrl);
        });

        builder.Services.AddSingleton<LiveEventClient>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
	}
}
