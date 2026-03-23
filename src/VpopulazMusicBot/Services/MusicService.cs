using Discord;
using Discord.WebSocket;
using Lavalink4NET;
using Lavalink4NET.Clients;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;
using VpopulazMusicBot.Services.Radio;

namespace VpopulazMusicBot.Services;

public sealed class MusicService(
    IAudioService audioService,
    IRadioStreamValidator radioStreamValidator,
    IOptions<QueuedLavalinkPlayerOptions> queuedPlayerOptions)
{
    public async Task<(bool Ok, string Message)> JoinAsync(
        SocketGuildUser user,
        CancellationToken cancellationToken = default)
    {
        var voiceChannel = user.VoiceChannel;

        if (voiceChannel is null)
            return (false, "Сначала зайди в голосовой канал");

        var playerResult = await GetOrCreateQueuedPlayer(
            user.Guild.Id,
            voiceChannel.Id,
            cancellationToken);

        return !playerResult.IsSuccess
            ? (false, $"Не удалось подключиться к Lavalink: {playerResult.Status.ToString()}")
            : (true, $"Подключился к каналу **{voiceChannel.Name}**");
    }

    public async Task<(bool Ok, string Message)> PlayAsync(
        IGuild guild,
        string query,
        CancellationToken cancellationToken = default)
    {
        var player = await audioService.Players.GetPlayerAsync(guild.Id, cancellationToken);

        if (player is null)
            return (false, $"Бот не подключен к голосовому каналу. Используй команду {Format.Code("/join")}");

        if (player is not IQueuedLavalinkPlayer queuedPlayer)
            return (false, "Плеер не поддерживает очередь");

        var track = await audioService.Tracks.LoadTrackAsync(
            query,
            TrackSearchMode.YouTube,
            default,
            cancellationToken);

        if (track is null)
            return (false, "Ничего не нашёл по запросу :(");

        await queuedPlayer.PlayAsync(track, enqueue: true, cancellationToken: cancellationToken);

        return (true, $"Добавил в очередь: **{track.Title}**");
    }

    public async Task<(bool Ok, string Message)> PlayStreamAsync(
        IGuild guild,
        string streamUrl,
        CancellationToken cancellationToken = default)
    {
        var player = await audioService.Players.GetPlayerAsync(guild.Id, cancellationToken);

        if (player is null)
            return (false, $"Бот не подключен к голосовому каналу. Используй команду {Format.Code("/join")}");

        var validationResult = await radioStreamValidator.ValidateAsync(streamUrl, cancellationToken);

        if (!validationResult.IsSuccessful)
            return (false, validationResult.Message ?? "Невалидный поток");

        var track = await audioService.Tracks.LoadTrackAsync(
            streamUrl,
            TrackSearchMode.None,
            cancellationToken: cancellationToken);

        if (track is null)
            return (false, "Ничего не нашёл по ссылке :(");

        await player.PlayAsync(track, cancellationToken: cancellationToken);

        return (true, "Запускаю поточек");
    }

    public async Task<(bool Ok, string Message)> SkipAsync(
        IGuild guild,
        CancellationToken cancellationToken = default)
    {
        var player = await audioService.Players.GetPlayerAsync(guild.Id, cancellationToken);

        if (player is null)
            return (false, "Бот не подключен к голосовому каналу");

        if (player is not IQueuedLavalinkPlayer queuedPlayer)
            return (false, "Плеер не поддерживает очередь");

        await queuedPlayer.SkipAsync(cancellationToken: cancellationToken);
        return (true, "Трек пропущен");
    }

    public async Task<(bool Ok, string Message)> StopAsync(
        IGuild guild,
        CancellationToken ct = default)
    {
        var player = await audioService.Players.GetPlayerAsync(guild.Id, ct);

        if (player is null)
            return (false, "Бот не подключен к голосовому каналу");

        if (player is IQueuedLavalinkPlayer queuedPlayer)
            await queuedPlayer.Queue.ClearAsync(ct);

        await player.StopAsync(ct);
        return (true, "Остановил воспроизведение и очистил очередь");
    }

    public async Task<(bool Ok, string Message)> LeaveAsync(
        IGuild guild,
        CancellationToken ct = default)
    {
        var player = await audioService.Players.GetPlayerAsync(guild.Id, ct);

        if (player is null)
            return (false, "Бот не подключен к голосовому каналу");

        await player.DisconnectAsync(ct);
        return (true, "Отключился от голосового канала");
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
            new PlayerRetrieveOptions
            {
                ChannelBehavior = PlayerChannelBehavior.Join,
                VoiceStateBehavior = MemberVoiceStateBehavior.RequireSame
            },
            cancellationToken);
}
