using VpopulazMusicBot;

var app = Host
    .CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(b => b.UseStartup<Startup>())
    .Build();

app.Run();