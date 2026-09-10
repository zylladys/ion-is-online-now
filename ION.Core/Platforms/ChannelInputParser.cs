using ION.Core.Models;

namespace ION.Core.Platforms;

public static class ChannelInputParser
{
    public static StreamChannel? Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var originalInput = input.Trim();
        var platform = PlatformDetector.Detect(originalInput);

        if (platform == StreamingPlatform.Unknown)
            return null;

        var normalizedInput = originalInput;

        if (!normalizedInput.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !normalizedInput.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            normalizedInput = $"https://{normalizedInput}";
        }

        if (!Uri.TryCreate(normalizedInput, UriKind.Absolute, out var uri))
            return null;

        var segments = uri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        var username = ExtractUsername(platform, segments);

        if (string.IsNullOrWhiteSpace(username))
            return null;

        username = username.TrimStart('@');

        return new StreamChannel
        {
            Platform = platform,
            Input = originalInput,
            Username = username,
            DisplayName = username,
            ChannelUrl = BuildCanonicalUrl(platform, username)
        };
    }

    private static string? ExtractUsername(
        StreamingPlatform platform,
        string[] segments)
    {
        if (segments.Length == 0)
            return null;

        return platform switch
        {
            StreamingPlatform.Twitch => segments[0],

            StreamingPlatform.Kick => segments[0],

            StreamingPlatform.Picarto => segments[0],

            StreamingPlatform.Piczel =>
                segments[0].Equals("watch", StringComparison.OrdinalIgnoreCase)
                    && segments.Length > 1
                        ? segments[1]
                        : segments[0],

            StreamingPlatform.TikTok => segments[0],

            StreamingPlatform.Instagram => segments[0],

            StreamingPlatform.YouTube =>
                segments[0].StartsWith("@")
                    ? segments[0]
                    : null,

            _ => null
        };
    }

    private static string BuildCanonicalUrl(
        StreamingPlatform platform,
        string username)
    {
        return platform switch
        {
            StreamingPlatform.Twitch =>
                $"https://twitch.tv/{username}",

            StreamingPlatform.YouTube =>
                $"https://youtube.com/@{username}",

            StreamingPlatform.Kick =>
                $"https://kick.com/{username}",

            StreamingPlatform.Picarto =>
                $"https://picarto.tv/{username}",

            StreamingPlatform.Piczel =>
                $"https://piczel.tv/watch/{username}",

            StreamingPlatform.TikTok =>
                $"https://tiktok.com/@{username}",

            StreamingPlatform.Instagram =>
                $"https://instagram.com/{username}",

            _ => string.Empty
        };
    }
}