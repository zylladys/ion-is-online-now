using ION.Core.Platforms;

namespace ION.Core.Models;

public class LiveEvent
{
    public StreamingPlatform Platform { get; set; }

    public string Username { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string ChannelUrl { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? GameName { get; set; }

    public int? ViewerCount { get; set; }

    public DateTimeOffset? StartedAt { get; set; }

    public DateTimeOffset DetectedAt { get; set; } =
        DateTimeOffset.UtcNow;
}