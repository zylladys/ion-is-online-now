#if ANDROID

namespace ION.App.Services;

public class AndroidNotificationService
    : INotificationService
{
    public Task ShowLiveNotificationAsync(
        string channelName,
        string? gameName,
        string? title,
        string channelUrl)
    {
        // Android notifications will be implemented here.
        return Task.CompletedTask;
    }
}

#endif