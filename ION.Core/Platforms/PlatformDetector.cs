namespace ION.Core.Platforms;

public static class PlatformDetector
{
    public static StreamingPlatform Detect(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return StreamingPlatform.Unknown;

        var value = input.Trim().ToLowerInvariant();

        if (value.Contains("twitch.tv"))
            return StreamingPlatform.Twitch;

        if (value.Contains("youtube.com") ||
            value.Contains("youtu.be"))
            return StreamingPlatform.YouTube;

        if (value.Contains("kick.com"))
            return StreamingPlatform.Kick;

        if (value.Contains("picarto.tv"))
            return StreamingPlatform.Picarto;

        if (value.Contains("piczel.tv"))
            return StreamingPlatform.Piczel;

        if (value.Contains("tiktok.com"))
            return StreamingPlatform.TikTok;

        if (value.Contains("instagram.com"))
            return StreamingPlatform.Instagram;

        return StreamingPlatform.Unknown;
    }
}