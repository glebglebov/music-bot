using JetBrains.Annotations;

namespace VpopulazMusicBot.Options;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class DiscordOptions
{
    public const string SectionName = "Discord";

    public required string Token { get; init; }
    public ulong GuildId { get; init; }
}
