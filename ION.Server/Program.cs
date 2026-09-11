using ION.Core.Models;
using ION.Server.Hubs;
using ION.Server.Integrations.Twitch;
using ION.Server.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .Configure<TwitchOptions>(
        builder.Configuration.GetSection(
            TwitchOptions.SectionName));

builder.Services.AddHttpClient<TwitchTokenService>();

builder.Services.AddHttpClient<TwitchService>();

builder.Services.AddSingleton<ServerChannelStore>();

builder.Services.AddSingleton<LiveEventDispatcher>();

builder.Services.AddHostedService<StreamMonitorService>();

builder.Services.AddSignalR();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet(
    "/api/twitch/{username}",
    async (
        string username,
        TwitchService twitchService) =>
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Results.BadRequest();
        }

        var status =
            await twitchService.GetStreamStatusAsync(
                username);

        return Results.Ok(status);
    });

app.MapHub<LiveHub>("/hubs/live");

app.MapGet(
    "/api/channels",
    async (
        ServerChannelStore channelStore) =>
    {
        var channels =
            await channelStore.GetAllAsync();

        return Results.Ok(channels);
    });

app.MapPost(
    "/api/channels",
    async (
        StreamChannel channel,
        ServerChannelStore channelStore) =>
    {
        if (string.IsNullOrWhiteSpace(
                channel.Username))
        {
            return Results.BadRequest(
                "Username is required.");
        }

        var added =
            await channelStore.AddAsync(
                channel);

        if (!added)
        {
            return Results.Conflict(
                "Channel already exists.");
        }

        return Results.Created(
            $"/api/channels/{channel.Id}",
            channel);
    });

app.MapDelete(
    "/api/channels/{id:guid}",
    async (
        Guid id,
        ServerChannelStore channelStore) =>
    {
        var removed =
            await channelStore.RemoveAsync(id);

        if (!removed)
            return Results.NotFound();

        return Results.NoContent();
    });

app.Run();