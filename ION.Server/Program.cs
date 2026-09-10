using ION.Server.Integrations.Twitch;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services
    .Configure<TwitchOptions>(
        builder.Configuration.GetSection(
            TwitchOptions.SectionName));

builder.Services.AddHttpClient<TwitchTokenService>();

builder.Services.AddHttpClient<TwitchService>();

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

app.Run();