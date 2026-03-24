using Discord.Interactions;
using JetBrains.Annotations;
using VpopulazMusicBot.Services;

namespace VpopulazMusicBot.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed class MusicModule(MusicService musicService) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("join", "Подключить бота к голосовому каналу", runMode: RunMode.Async)]
    public async Task JoinAsync()
    {
        await DeferAsync();

        var user = Context.Guild.GetUser(Context.User.Id);

        var result = await musicService.JoinAsync(user);
        await FollowupAsync(result.Message);
    }

    [SlashCommand("play", "Добавить трек по URL или поисковому запросу")]
    public async Task PlayAsync([Summary("query", "YouTube URL или поисковый запрос")] string query)
    {
        await DeferAsync();

        var result = await musicService.PlayAsync(Context.Guild, query);
        await FollowupAsync(result.Message);
    }

    [SlashCommand("play-stream", "Запустить аудиопоток")]
    public async Task PlayStreamAsync([Summary("url", "URL потока")] string query)
    {
        await DeferAsync();

        var result = await musicService.PlayStreamAsync(Context.Guild, query);
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

    [SlashCommand("test", "Эксперименты")]
    public async Task TestAsync()
    {
        await DeferAsync();

        await RespondAsync("respond");

        await Task.Delay(2_000);

        await FollowupAsync("followup 1");

        await Task.Delay(2_000);

        await FollowupAsync("followup 2");
    }
}
