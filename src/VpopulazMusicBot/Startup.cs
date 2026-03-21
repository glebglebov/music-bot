using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Extensions;
using VpopulazMusicBot.Options;
using VpopulazMusicBot.Services;

namespace VpopulazMusicBot;

public class Startup(IConfiguration configuration)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<DiscordOptions>(configuration.GetSection(DiscordOptions.SectionName));
        services.Configure<LavalinkOptions>(configuration.GetSection(LavalinkOptions.SectionName));

        services.AddSingleton(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildVoiceStates,
            LogGatewayIntentWarnings = false,
            AlwaysDownloadUsers = false
        });

        services.AddSingleton<DiscordSocketClient>();
        services.AddSingleton(sp => new InteractionService(sp.GetRequiredService<DiscordSocketClient>()));

        services.AddLavalink();

        services.Configure<LavalinkNodeOptions>(options =>
        {
            var cfg = configuration
                .GetSection(LavalinkOptions.SectionName)
                .Get<LavalinkOptions>()!;

            options.BaseAddress = new Uri($"http://{cfg.Host}:{cfg.Port}");
            options.Passphrase = cfg.Pass;
        });

        services.Configure<LavalinkClusterOptions>(_ => { });

        services.AddSingleton<MusicService>();
        services.AddHostedService<BotHostedService>();

        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        => app
            .UseCors()
            .UseRouting()
            .UseEndpoints(e =>
            {
                e.MapControllers();
            });
}
