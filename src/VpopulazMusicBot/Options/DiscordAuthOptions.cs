using JetBrains.Annotations;

namespace VpopulazMusicBot.Options;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class DiscordAuthOptions
{
    public required string Token { get; init; }
}
