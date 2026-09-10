using System.Text.Json;

using ION.Core.Models;

namespace ION.Core.Services;

public static class ChannelStore
{
    private static readonly List<StreamChannel> _channels = [];

    private static string? _filePath;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static IReadOnlyList<StreamChannel> Channels => _channels;

    public static void Initialize(string storageDirectory)
    {
        if (string.IsNullOrWhiteSpace(storageDirectory))
            throw new ArgumentException(
                "Storage directory cannot be empty.",
                nameof(storageDirectory));

        Directory.CreateDirectory(storageDirectory);

        _filePath = Path.Combine(
            storageDirectory,
            "channels.json");

        Load();
    }

    public static async Task AddAsync(StreamChannel channel)
    {
        EnsureInitialized();

        var duplicate = _channels.Any(x =>
            x.Platform == channel.Platform &&
            string.Equals(
                x.Username,
                channel.Username,
                StringComparison.OrdinalIgnoreCase));

        if (duplicate)
            return;

        _channels.Add(channel);

        await SaveAsync();
    }

    public static async Task RemoveAsync(Guid id)
    {
        EnsureInitialized();

        var channel = _channels.FirstOrDefault(x => x.Id == id);

        if (channel is null)
            return;

        _channels.Remove(channel);

        await SaveAsync();
    }

    private static void Load()
    {
        EnsureInitialized();

        _channels.Clear();

        if (!File.Exists(_filePath))
            return;

        try
        {
            var json = File.ReadAllText(_filePath);

            var channels =
                JsonSerializer.Deserialize<List<StreamChannel>>(
                    json,
                    JsonOptions);

            if (channels is not null)
                _channels.AddRange(channels);
        }
        catch
        {
            // If the file is corrupted, start with an empty list.
            // Proper error logging can be added later.
            _channels.Clear();
        }
    }

    private static async Task SaveAsync()
    {
        EnsureInitialized();

        var json = JsonSerializer.Serialize(
            _channels,
            JsonOptions);

        await File.WriteAllTextAsync(
            _filePath!,
            json);
    }

    private static void EnsureInitialized()
    {
        if (string.IsNullOrWhiteSpace(_filePath))
        {
            throw new InvalidOperationException(
                "ChannelStore has not been initialized.");
        }
    }
}