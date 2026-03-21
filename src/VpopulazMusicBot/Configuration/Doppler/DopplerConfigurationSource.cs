namespace VpopulazMusicBot.Configuration.Doppler;

public sealed class DopplerConfigurationSource(string projectName) : IConfigurationSource
{
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new DopplerConfigurationProvider(projectName);
    }
}
