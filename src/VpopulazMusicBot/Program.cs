using VpopulazMusicBot;
using VpopulazMusicBot.Configuration.Doppler;

var app = Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(b => b
        .ConfigureAppConfiguration((_, cb) => cb.Add(new DopplerConfigurationSource("vpopulaz-music-bot")))
        .UseStartup<Startup>())
    .Build();

app.Run();
