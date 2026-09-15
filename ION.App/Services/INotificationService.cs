namespace ION.App.Services;

public interface INotificationService
{
    Task ShowLiveNotificationAsync(
        string channelName,
        string? gameName,
        string? title,
        string channelUrl);
}