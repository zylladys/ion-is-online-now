using System.Text.Json;

using ION.Core.Models;

namespace ION.Server.Services;

public class ServerChannelStore
{
    private readonly List<StreamChannel> _channels = [];
    private readonly SemaphoreSlim _lock = new(1, 1);

    private readonly string _filePath;

    private static readonly JsonSerializerOptions JsonOptions =
        new()
        {
            WriteIndented = true
        };

    public ServerChannelStore(
        IWebHostEnvironment environment)
    {
        var dataDirectory =
            Path.Combine(
                environment.ContentRootPath,
                "Data");

        Directory.CreateDirectory(dataDirectory);

        _filePath =
            Path.Combine(
                dataDirectory,
                "channels.json");

        Load();
    }

    public async Task<IReadOnlyList<StreamChannel>>
        GetAllAsync()
    {
        await _lock.WaitAsync();

        try
        {
            return _channels
                .Select(Clone)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> AddAsync(
        StreamChannel channel)
    {
        await _lock.WaitAsync();

        try
        {
            var duplicate =
                _channels.Any(x =>
                    x.Platform == channel.Platform &&
                    string.Equals(
                        x.Username,
                        channel.Username,
                        StringComparison.OrdinalIgnoreCase));

            if (duplicate)
                return false;

            _channels.Add(channel);

            await SaveAsync();

            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> RemoveAsync(
        Guid id)
    {
        await _lock.WaitAsync();

        try
        {
            var channel =
                _channels.FirstOrDefault(
                    x => x.Id == id);

            if (channel is null)
                return false;

            _channels.Remove(channel);

            await SaveAsync();

            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private void Load()
    {
        if (!File.Exists(_filePath))
            return;

        try
        {
            var json =
                File.ReadAllText(_filePath);

            var channels =
                JsonSerializer.Deserialize<
                    List<StreamChannel>>(
                    json,
                    JsonOptions);

            if (channels is not null)
            {
                _channels.AddRange(channels);
            }
        }
        catch (Exception)
        {
            _channels.Clear();
        }
    }

    private async Task SaveAsync()
    {
        var json =
            JsonSerializer.Serialize(
                _channels,
                JsonOptions);

        await File.WriteAllTextAsync(
            _filePath,
            json);
    }

    public async Task<bool> UpdateLiveStateAsync(
    Guid channelId,
    bool isLive)
    {
        await _lock.WaitAsync();

        try
        {
            var channel =
                _channels.FirstOrDefault(
                    x => x.Id == channelId);

            if (channel is null)
                return false;

            if (channel.IsLive == isLive)
                return true;

            channel.IsLive = isLive;

            await SaveAsync();

            return true;
        }
        finally
        {
            _lock.Release();
        }
    }

    private static StreamChannel Clone(
        StreamChannel channel)
    {
        return new StreamChannel
        {
            Id = channel.Id,
            Platform = channel.Platform,
            Input = channel.Input,
            PlatformChannelId =
                channel.PlatformChannelId,
            Username = channel.Username,
            DisplayName = channel.DisplayName,
            ChannelUrl = channel.ChannelUrl,
            AvatarUrl = channel.AvatarUrl,
            IsLive = channel.IsLive,
            NotificationsEnabled =
                channel.NotificationsEnabled
        };
    }
}