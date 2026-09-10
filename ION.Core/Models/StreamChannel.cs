using ION.Core.Platforms;

namespace ION.Core.Models;

public class StreamChannel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public StreamingPlatform Platform { get; set; }

    public string Input { get; set; } = string.Empty;

    public string? PlatformChannelId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string ChannelUrl { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public bool IsLive { get; set; }

    public bool NotificationsEnabled { get; set; } = true;
}