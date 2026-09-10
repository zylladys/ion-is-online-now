using System.Net.Http.Json;

using Microsoft.Extensions.Options;

using System.Text.Json.Serialization;

namespace ION.Server.Integrations.Twitch;

public class TwitchTokenService
{
    private readonly HttpClient _httpClient;
    private readonly TwitchOptions _options;

    private string? _accessToken;
    private DateTimeOffset _expiresAt;

    public TwitchTokenService(
        HttpClient httpClient,
        IOptions<TwitchOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_accessToken) &&
            DateTimeOffset.UtcNow < _expiresAt)
        {
            return _accessToken;
        }

        var url =
            "https://id.twitch.tv/oauth2/token" +
            $"?client_id={Uri.EscapeDataString(_options.ClientId)}" +
            $"&client_secret={Uri.EscapeDataString(_options.ClientSecret)}" +
            "&grant_type=client_credentials";

        using var response =
            await _httpClient.PostAsync(url, null);

        response.EnsureSuccessStatusCode();

        var tokenResponse =
            await response.Content.ReadFromJsonAsync<TwitchTokenResponse>();

        if (tokenResponse is null ||
            string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException(
                "Twitch did not return an access token.");
        }

        _accessToken = tokenResponse.AccessToken;

        _expiresAt =
            DateTimeOffset.UtcNow.AddSeconds(
                Math.Max(tokenResponse.ExpiresIn - 60, 60));

        return _accessToken;
    }

    private sealed class TwitchTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;
    }
}