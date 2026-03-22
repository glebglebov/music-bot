using Discord.Interactions;
using VpopulazMusicBot.Services;

namespace VpopulazMusicBot.Modules;

public sealed class MusicModule(MusicService musicService) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("join", "Подключить бота к вашему голосовому каналу", runMode: RunMode.Async)]
    public async Task JoinAsync()
    {
        await DeferAsync();

        var user = Context.Guild.GetUser(Context.User.Id);

        var result = await musicService.JoinAsync(user);
        await FollowupAsync(result.Message);
    }

    [SlashCommand("play", "Проиграть музыку по URL или поисковому запросу")]
    public async Task PlayAsync([Summary("query", "YouTube URL или поисковый запрос")] string query)
    {
        await DeferAsync();

        var user = Context.Guild.GetUser(Context.User.Id);

        var result = await musicService.PlayAsync(user, query);
        await FollowupAsync(result.Message);
    }

    [SlashCommand("skip", "Пропустить текущий трек")]
    public async Task SkipAsync()
    {
        await DeferAsync();

        var result = await musicService.SkipAsync(Context.Guild);
        await FollowupAsync(result.Message);
    }

    [SlashCommand("stop", "Остановить музыку и очистить очередь")]
    public async Task StopAsync()
    {
        await DeferAsync();

        var result = await musicService.StopAsync(Context.Guild);
        await FollowupAsync(result.Message);
    }

    [SlashCommand("leave", "Отключить бота от голосового канала")]
    public async Task LeaveAsync()
    {
        await DeferAsync();

        var result = await musicService.LeaveAsync(Context.Guild);
        await FollowupAsync(result.Message);
    }
}
