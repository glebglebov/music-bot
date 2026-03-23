using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET.Extensions;
using Lavalink4NET.Players.Queued;
using VpopulazMusicBot.Options;
using VpopulazMusicBot.Services;
using VpopulazMusicBot.Services.Radio;

namespace VpopulazMusicBot;

public class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .Configure<DiscordAuthOptions>(configuration.GetSection(ConfigSections.Discord.Auth))
            .Configure<LavalinkConnectionOptions>(configuration.GetSection(ConfigSections.Lavalink.Connection));

        services
            .AddSingleton(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildVoiceStates,
                LogGatewayIntentWarnings = false,
                AlwaysDownloadUsers = false
            })
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(sp => new InteractionService(sp.GetRequiredService<DiscordSocketClient>()));

        services
            .ConfigureLavalink(options =>
            {
                var cfg = configuration
                    .GetSection(ConfigSections.Lavalink.Connection)
                    .Get<LavalinkConnectionOptions>()!;

                options.BaseAddress = new Uri($"http://{cfg.Host}:{cfg.Port}");
                options.Passphrase = cfg.Password;
            })
            .Configure<QueuedLavalinkPlayerOptions>(configuration.GetSection(ConfigSections.Lavalink.Players.Queued))
            .AddLavalink();

        services
            .AddSingleton<MusicService>()
            .AddSingleton<IRadioStreamValidator, RadioStreamValidator>();

        services.AddHostedService<BotHostedService>();

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        => app
            .UseRouting()
            .UseEndpoints(e =>
            {
                e.MapControllers();
            });
}
