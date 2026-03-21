namespace VpopulazMusicBot;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
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