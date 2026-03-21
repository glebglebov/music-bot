using Discord;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;

namespace VpopulazMusicBot.Services;

public sealed class MusicService(
    IAudioService audioService,
    IOptions<QueuedLavalinkPlayerOptions> queuedPlayerOptions)
{
    public async Task<(bool Ok, string Message)> JoinAsync(
        SocketGuildUser user,
        CancellationToken cancellationToken = default)
    {
        var voiceChannel = user.VoiceChannel;

        if (voiceChannel is null)
            return (false, "Сначала зайди в голосовой канал.");

        var playerResult = await GetOrCreateQueuedPlayer(
            user.Guild.Id,
            voiceChannel.Id,
            cancellationToken);

        return !playerResult.IsSuccess
            ? (false, "Не удалось подключиться к Lavalink player.")
            : (true, $"Подключился к **{voiceChannel.Name}**.");
    }

    public async Task<(bool Ok, string Message)> PlayAsync(
        SocketGuildUser user,
        string query,
        CancellationToken cancellationToken = default)
    {
        var voiceChannel = user.VoiceChannel;

        if (voiceChannel is null)
            return (false, "Сначала зайди в голосовой канал.");

        var playerResult = await GetOrCreateQueuedPlayer(
            user.Guild.Id,
            voiceChannel.Id,
            cancellationToken);

        if (!playerResult.IsSuccess)
            return (false, "Не удалось создать или получить player.");

        var track = await audioService.Tracks.LoadTrackAsync(
            query,
            TrackSearchMode.YouTube,
            default,
            cancellationToken);

        if (track is null)
            return (false, "Ничего не нашёл по запросу.");

        await playerResult.Player.PlayAsync(track, enqueue: true, cancellationToken: cancellationToken);

        return (true, $"Добавил в очередь: **{track.Title}**");
    }

    public async Task<(bool Ok, string Message)> SkipAsync(
        IGuild guild,
        CancellationToken cancellationToken = default)
    {
        var player = await audioService.Players.GetPlayerAsync(guild.Id, cancellationToken);

        if (player is null)
            return (false, "Плеер не найден.");

        if (player is not IQueuedLavalinkPlayer queuedPlayer)
            return (false, "Текущий player не поддерживает очередь.");

        await queuedPlayer.SkipAsync(cancellationToken: cancellationToken);
        return (true, "Трек пропущен.");
    }

    public async Task<(bool Ok, string Message)> StopAsync(
        IGuild guild,
        CancellationToken ct = default)
    {
        var player = await audioService.Players.GetPlayerAsync(guild.Id, ct);

        if (player is null)
            return (false, "Плеер не найден.");

        if (player is IQueuedLavalinkPlayer queuedPlayer)
            await queuedPlayer.Queue.ClearAsync(ct);

        await player.StopAsync(ct);
        return (true, "Остановил воспроизведение и очистил очередь.");
    }

    public async Task<(bool Ok, string Message)> LeaveAsync(
        IGuild guild,
        CancellationToken ct = default)
    {
        var player = await audioService.Players.GetPlayerAsync(guild.Id, ct);

        if (player is null)
            return (false, "Плеер не найден.");

        await player.DisconnectAsync(ct);
        return (true, "Отключился от голосового канала.");
    }

    private async ValueTask<PlayerResult<QueuedLavalinkPlayer>> GetOrCreateQueuedPlayer(
        ulong guildId,
        ulong voiceChannelId,
        CancellationToken cancellationToken)
        => await audioService.Players.RetrieveAsync(
            guildId,
            voiceChannelId,
            PlayerFactory.Queued,
            queuedPlayerOptions,
            cancellationToken: cancellationToken);
}
