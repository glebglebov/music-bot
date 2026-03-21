using Discord.Interactions;
using Discord.WebSocket;
using VpopulazMusicBot.Services;

namespace VpopulazMusicBot.Modules;

public sealed class MusicModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly MusicService _musicService;

    public MusicModule(MusicService musicService)
    {
        _musicService = musicService;
    }

    [SlashCommand("join", "Подключить бота к вашему голосовому каналу")]
    public async Task JoinAsync()
    {
        await DeferAsync();

        var result = await _musicService.JoinAsync((SocketGuildUser)Context.User);
        await FollowupAsync(result.Message);
    }

    [SlashCommand("play", "Проиграть музыку по URL или поисковому запросу")]
    public async Task PlayAsync([Summary("query", "YouTube URL или поисковый запрос")] string query)
    {
        await DeferAsync();

        var result = await _musicService.PlayAsync((SocketGuildUser)Context.User, query);
        await FollowupAsync(result.Message);
    }

    [SlashCommand("skip", "Пропустить текущий трек")]
    public async Task SkipAsync()
    {
        await DeferAsync();

        var result = await _musicService.SkipAsync(Context.Guild);
        await FollowupAsync(result.Message);
    }

    [SlashCommand("stop", "Остановить музыку и очистить очередь")]
    public async Task StopAsync()
    {
        await DeferAsync();

        var result = await _musicService.StopAsync(Context.Guild);
        await FollowupAsync(result.Message);
    }

    [SlashCommand("leave", "Отключить бота от голосового канала")]
    public async Task LeaveAsync()
    {
        await DeferAsync();

        var result = await _musicService.LeaveAsync(Context.Guild);
        await FollowupAsync(result.Message);
    }
}
