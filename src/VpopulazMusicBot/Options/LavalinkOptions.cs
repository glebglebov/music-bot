namespace VpopulazMusicBot.Options;

public class LavalinkOptions
{
    public const string SectionName = "Lavalink";

    public string Host { get; set; } = "localhost";
    public ushort Port { get; set; } = 2333;
    public string Pass { get; set; } = "youshallnotpass";
    public string Label { get; set; } = "main-node";
}
