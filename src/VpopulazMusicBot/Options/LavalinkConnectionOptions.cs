using JetBrains.Annotations;

namespace VpopulazMusicBot.Options;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class LavalinkConnectionOptions
{
    public required string Host { get; init; }
    public ushort Port { get; init; }
    public required string Password { get; init; }
    public required string Label { get; init; }
}
