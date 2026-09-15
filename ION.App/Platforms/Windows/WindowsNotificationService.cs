using ION.App.Services;

using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;

namespace ION.App.Platforms.Windows;

public class WindowsNotificationService
    : INotificationService
{
    public WindowsNotificationService()
    {
        AppNotificationManager.Default.NotificationInvoked +=
            OnNotificationInvoked;

        AppNotificationManager.Default.Register();
    }

    private void OnNotificationInvoked(
    AppNotificationManager sender,
    AppNotificationActivatedEventArgs args)
    {
        System.Diagnostics.Debug.WriteLine(
            $"ION notification clicked: {args.Argument}");

        if (!args.Arguments.TryGetValue(
                "action",
                out var action) ||
            action != "openStream")
        {
            return;
        }

        if (!args.Arguments.TryGetValue(
                "url",
                out var url) ||
            string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(
            async () =>
            {
                try
                {
                    if (!Uri.TryCreate(
                            url,
                            UriKind.Absolute,
                            out var uri))
                    {
                        return;
                    }

                    await Browser.Default.OpenAsync(
                        uri,
                        BrowserLaunchMode.SystemPreferred);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"ION notification activation error: {ex}");
                }
            });
    }



    public Task ShowLiveNotificationAsync(
        string channelName,
        string? gameName,
        string? title,
        string channelUrl)
    {
        var builder =
            new AppNotificationBuilder()
                .AddArgument(
                    "action",
                    "openStream")
                .AddArgument(
                    "url",
                    channelUrl)
                .AddText(
                    $"{channelName} is live!");

        if (!string.IsNullOrWhiteSpace(gameName))
        {
            builder.AddText(gameName);
        }

        if (!string.IsNullOrWhiteSpace(title))
        {
            builder.AddText(title);
        }

        var notification =
            builder.BuildNotification();

        AppNotificationManager.Default.Show(
            notification);

        return Task.CompletedTask;
    }

    
    
}