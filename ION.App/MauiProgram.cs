using Microsoft.Extensions.Logging;

using ION.Core.Services;

namespace ION.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

        ChannelStore.Initialize(
            FileSystem.AppDataDirectory
            );

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
	}
}
