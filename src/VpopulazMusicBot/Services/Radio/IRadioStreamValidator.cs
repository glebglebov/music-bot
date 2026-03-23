namespace VpopulazMusicBot.Services.Radio;

public interface IRadioStreamValidator
{
    Task<ExecutionResult> ValidateAsync(string input, CancellationToken cancellationToken = default);
}
