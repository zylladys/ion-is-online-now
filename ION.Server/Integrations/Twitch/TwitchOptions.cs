namespace ION.Server.Integrations.Twitch;

public class TwitchOptions
{
    public const string SectionName = "Twitch";

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;
}