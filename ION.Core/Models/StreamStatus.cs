using ION.Core.Platforms;

namespace ION.Core.Models;

public class StreamStatus
{
    public StreamingPlatform Platform { get; set; }

    public string Username { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public bool IsLive { get; set; }

    public string? Title { get; set; }

    public string? GameName { get; set; }

    public int? ViewerCount { get; set; }

    public DateTimeOffset? StartedAt { get; set; }
}