namespace VpopulazMusicBot.Options;

public class DiscordOptions
{
    public const string SectionName = "Discord";

    public string Token { get; set; } = string.Empty;
    public ulong GuildId { get; set; }
}
