using ION.Core.Platforms;

namespace ION.Core.Models;

public class OfflineEvent
{
    public StreamingPlatform Platform { get; set; }

    public string Username { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string ChannelUrl { get; set; } = string.Empty;

    public DateTimeOffset DetectedAt { get; set; } =
        DateTimeOffset.UtcNow;
}