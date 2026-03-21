using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using VpopulazMusicBot.Modules;
using VpopulazMusicBot.Options;

namespace VpopulazMusicBot;

public class BotHostedService(
    DiscordSocketClient client,
    InteractionService interactions,
    IServiceProvider services,
    IOptions<DiscordOptions> discordOptions)
    : IHostedService
{
    private readonly DiscordOptions _discordOptions = discordOptions.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        client.Log += LogAsync;
        interactions.Log += LogAsync;

        client.Ready += OnReadyAsync;
        client.InteractionCreated += OnInteractionCreatedAsync;

        await interactions.AddModuleAsync<MusicModule>(services);

        await client.LoginAsync(TokenType.Bot, _discordOptions.Token);
        await client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await client.StopAsync();
        await client.LogoutAsync();
    }

    private async Task OnReadyAsync()
    {
        if (_discordOptions.GuildId != 0)
        {
            await interactions.RegisterCommandsToGuildAsync(_discordOptions.GuildId, true);
        }
        else
        {
            await interactions.RegisterCommandsGloballyAsync(true);
        }
    }

    private async Task OnInteractionCreatedAsync(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(client, interaction);
            await interactions.ExecuteCommandAsync(context, services);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            if (interaction.Type == InteractionType.ApplicationCommand)
            {
                await interaction.GetOriginalResponseAsync()
                    .ContinueWith(async msgTask =>
                    {
                        if (msgTask.IsCompletedSuccessfully)
                        {
                            await (await msgTask).DeleteAsync();
                        }
                    });
            }
        }
    }

    private static Task LogAsync(LogMessage message)
    {
        Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss}] {message.Severity}: {message.Source} - {message.Message}");
        if (message.Exception is not null)
        {
            Console.WriteLine(message.Exception);
        }

        return Task.CompletedTask;
    }
}
