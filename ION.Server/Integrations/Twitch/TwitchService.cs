using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Options;

using ION.Core.Models;
using ION.Core.Platforms;

namespace ION.Server.Integrations.Twitch;

public class TwitchService
{
    private readonly HttpClient _httpClient;
    private readonly TwitchTokenService _tokenService;
    private readonly TwitchOptions _options;

    public TwitchService(
        HttpClient httpClient,
        TwitchTokenService tokenService,
        IOptions<TwitchOptions> options)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _options = options.Value;
    }

    public async Task<StreamStatus> GetStreamStatusAsync(
        string username)
    {
        var token =
            await _tokenService.GetAccessTokenAsync();

        var url =
            "https://api.twitch.tv/helix/streams" +
            $"?user_login={Uri.EscapeDataString(username)}";

        using var request =
            new HttpRequestMessage(
                HttpMethod.Get,
                url);

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        request.Headers.Add(
            "Client-Id",
            _options.ClientId);

        using var response =
            await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var twitchResponse =
            await response.Content.ReadFromJsonAsync<TwitchStreamsResponse>();

        var stream =
            twitchResponse?.Data.FirstOrDefault();

        if (stream is null)
        {
            return new StreamStatus
            {
                Platform = StreamingPlatform.Twitch,
                Username = username,
                IsLive = false
            };
        }

        return new StreamStatus
        {
            Platform = StreamingPlatform.Twitch,
            Username = stream.UserLogin,
            DisplayName = stream.UserName,
            IsLive = true,
            Title = stream.Title,
            GameName = stream.GameName,
            ViewerCount = stream.ViewerCount,
            StartedAt = stream.StartedAt
        };

    }

    private sealed class TwitchStreamsResponse
    {
        [JsonPropertyName("data")]
        public List<TwitchStreamData> Data { get; set; } = [];
    }

    private sealed class TwitchStreamData
    {
        [JsonPropertyName("user_login")]
        public string UserLogin { get; set; } = string.Empty;

        [JsonPropertyName("user_name")]
        public string UserName { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("game_name")]
        public string GameName { get; set; } = string.Empty;

        [JsonPropertyName("viewer_count")]
        public int ViewerCount { get; set; }

        [JsonPropertyName("started_at")]
        public DateTimeOffset StartedAt { get; set; }
    }
}
