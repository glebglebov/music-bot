namespace VpopulazMusicBot;

public static class ConfigSections
{
    public static class Lavalink
    {
        private const string LavalinkPrefix = "Lavalink";

        public const string Connection = $"{LavalinkPrefix}:{nameof(Connection)}";

        public static class Players
        {
            private const string PlayersPrefix = "Players";

            public const string Queued = $"{LavalinkPrefix}:{PlayersPrefix}:{nameof(Queued)}";
        }
    }

    public static class Discord
    {
        private const string DiscordPrefix = "Discord";

        public const string Auth = $"{DiscordPrefix}:{nameof(Auth)}";
    }
}
