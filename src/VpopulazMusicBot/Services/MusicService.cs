using Discord;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;

namespace VpopulazMusicBot.Services;

public sealed class MusicService(IAudioService audioService)
{
    public async Task<(bool Ok, string Message)> JoinAsync(SocketGuildUser user, CancellationToken ct = default)
    {
        var voiceChannel = user.VoiceChannel;

        if (voiceChannel is null)
            return (false, "Сначала зайди в голосовой канал.");

        var player = await audioService.Players.RetrieveAsync(
            user.Guild.Id,
            user.VoiceChannel.Id,
            PlayerFactory.Queued,
            ct);

        return !player.IsSuccess
            ? (false, "Не удалось подключиться к Lavalink player.")
            : (true, $"Подключился к **{voiceChannel.Name}**.");
    }

    public async Task<(bool Ok, string Message)> PlayAsync(SocketGuildUser user, string query, CancellationToken ct = default)
    {
        var voiceChannel = user.VoiceChannel;

        if (voiceChannel is null)
            return (false, "Сначала зайди в голосовой канал.");

        var playerResult = await audioService.Players.RetrieveAsync(
            user.Guild.Id,
            voiceChannel.Id,
            PlayerFactory.Queued,
            ct);

        if (!playerResult.IsSuccess)
            return (false, "Не удалось создать или получить player.");

        var player = playerResult.Player;

        var loadResult = await audioService.Tracks.LoadTrackAsync(query, TrackSearchMode.YouTube, ct);

        if (loadResult is null)
            return (false, "Ничего не нашёл по запросу.");

        await player.PlayAsync(loadResult, enqueue: true, cancellationToken: ct);

        return (true, $"Добавил в очередь: **{loadResult.Title}**");
    }

    public async Task<(bool Ok, string Message)> SkipAsync(IGuild guild, CancellationToken ct = default)
    {
        var player = await audioService.Players.GetPlayerAsync(guild.Id, ct);
        if (player is null)
        {
            return (false, "Плеер не найден.");
        }

        if (player is not IQueuedLavalinkPlayer queuedPlayer)
        {
            return (false, "Текущий player не поддерживает очередь.");
        }

        await queuedPlayer.SkipAsync(ct);
        return (true, "Трек пропущен.");
    }

    public async Task<(bool Ok, string Message)> StopAsync(IGuild guild, CancellationToken ct = default)
    {
        var player = await audioService.Players.GetPlayerAsync(guild.Id, ct);
        if (player is null)
        {
            return (false, "Плеер не найден.");
        }

        if (player is IQueuedLavalinkPlayer queuedPlayer)
        {
            await queuedPlayer.Queue.ClearAsync(ct);
        }

        await player.StopAsync(ct);
        return (true, "Остановил воспроизведение и очистил очередь.");
    }

    public async Task<(bool Ok, string Message)> LeaveAsync(IGuild guild, CancellationToken ct = default)
    {
        var player = await audioService.Players.GetPlayerAsync(guild.Id, ct);
        if (player is null)
        {
            return (false, "Плеер не найден.");
        }

        await player.DisconnectAsync(ct);
        return (true, "Отключился от голосового канала.");
    }
}
