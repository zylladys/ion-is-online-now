using System.Net;
using System.Net.Http.Json;

using ION.Core.Models;

namespace ION.App.Services;

public class IonApiClient
{
    private readonly HttpClient _httpClient;

    public IonApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<StreamStatus?> GetTwitchStatusAsync(
        string username)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<StreamStatus>(
                $"/api/twitch/{Uri.EscapeDataString(username)}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<StreamChannel>>
        GetChannelsAsync()
    {
        try
        {
            var channels =
                await _httpClient.GetFromJsonAsync<
                    List<StreamChannel>>(
                    "/api/channels");

            return channels ?? [];
        }
        catch
        {
            return [];
        }
    }

    public async Task<bool> AddChannelAsync(
        StreamChannel channel)
    {
        try
        {
            using var response =
                await _httpClient.PostAsJsonAsync(
                    "/api/channels",
                    channel);

            if (response.StatusCode ==
                HttpStatusCode.Conflict)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RemoveChannelAsync(
        Guid id)
    {
        try
        {
            using var response =
                await _httpClient.DeleteAsync(
                    $"/api/channels/{id}");

            if (response.StatusCode ==
                HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();

            return true;
        }
        catch
        {
            return false;
        }
    }
}