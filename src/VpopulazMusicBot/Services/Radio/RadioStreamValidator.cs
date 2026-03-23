using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace VpopulazMusicBot.Services.Radio;

public sealed class RadioStreamValidator : IRadioStreamValidator
{
    private static readonly HashSet<string> Mp3MimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "audio/mpeg",
        "audio/mp3"
    };

    private static readonly HashSet<string> PlaylistMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "audio/x-mpegurl",
        "application/vnd.apple.mpegurl",
        "application/x-mpegurl",
        "audio/mpegurl",
        "audio/scpls",
        "application/pls+xml"
    };

    private static readonly HashSet<string> GenericAudioMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "audio/aac",
        "audio/aacp",
        "audio/ogg",
        "audio/webm",
        "audio/flac",
        "application/octet-stream"
    };

    private readonly HttpClient _httpClient;

    public RadioStreamValidator()
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        _httpClient = new HttpClient(handler);
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    public async Task<ExecutionResult> ValidateAsync(string input, CancellationToken cancellationToken = default)
    {
        if (!IsValidUrl(input, out var uri))
            return new ExecutionResult
            {
                IsSuccessful = false,
                Message = "Некорректный URL"
            };

        try
        {
            using var headRequest = new HttpRequestMessage(HttpMethod.Head, uri);

            using var headResponse = await _httpClient.SendAsync(
                headRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (headResponse.IsSuccessStatusCode)
            {
                var isValid = IsValidStream(headResponse);

                if (isValid)
                    return new ExecutionResult
                    {
                        IsSuccessful = true
                    };
            }
        }
        catch
        {
            // ignored
        }

        try
        {
            using var getRequest = new HttpRequestMessage(HttpMethod.Get, uri);

            using var getResponse = await _httpClient.SendAsync(
                getRequest,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!getResponse.IsSuccessStatusCode)
                return new ExecutionResult
                {
                    IsSuccessful = false,
                    Message = $"Сервер вернул {(int)getResponse.StatusCode} {getResponse.ReasonPhrase}"
                };

            var isValid = IsValidStream(getResponse);

            if (isValid)
                return new ExecutionResult
                {
                    IsSuccessful = true
                };
        }
        catch (Exception ex)
        {
            return new ExecutionResult
            {
                IsSuccessful = false,
                Message = ex.Message
            };
        }

        return new ExecutionResult
        {
            IsSuccessful = false,
            Message = "Передан некорректный поток"
        };
    }

    private static bool IsValidUrl(string url, [NotNullWhen(true)] out Uri? uri)
    {
        var result = Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                     && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

        uri = uriResult;
        return result;
    }

    private static bool IsValidStream(HttpResponseMessage response)
    {
        var contentType = response.Content.Headers.ContentType?.MediaType;

        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        if (Mp3MimeTypes.Contains(contentType))
            return true;

        if (PlaylistMimeTypes.Contains(contentType))
            return true;

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (GenericAudioMimeTypes.Contains(contentType) ||
            contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
