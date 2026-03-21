using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace VpopulazMusicBot.Configuration.Doppler;

public sealed class DopplerConfigurationProvider(string projectName) : ConfigurationProvider
{
    private const string BaseUrl = "https://api.doppler.com";
    private const string Config = "prd";
    private const string SecretName = "JsonConfig";

    public override void Load()
    {
        LoadAsync().GetAwaiter().GetResult();
    }

    private async Task LoadAsync()
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(BaseUrl);

        var dopplerToken = Environment.GetEnvironmentVariable("DOPPLER_TOKEN")
                           ?? throw new InvalidOperationException("DOPPLER_TOKEN is not set");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", dopplerToken);

        var url =
            $"/v3/configs/config/secrets/download?project={Uri.EscapeDataString(projectName)}" +
            $"&config={Uri.EscapeDataString(Config)}" +
            $"&format=dotnet-json";

        using var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var secrets = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json)
                      ?? throw new InvalidOperationException("Doppler returned empty JSON");

        if (!secrets.TryGetValue(SecretName, out var secretValue) || secretValue.ValueKind != JsonValueKind.String)
            throw new InvalidOperationException($"Secret '{SecretName}' was not found or is not a string");

        var embeddedJson = secretValue.GetString();

        if (string.IsNullOrWhiteSpace(embeddedJson))
            throw new InvalidOperationException($"Secret '{SecretName}' is empty");

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(embeddedJson));

        var tempBuilder = new ConfigurationBuilder();
        tempBuilder.AddJsonStream(stream);

        var tempConfig = tempBuilder.Build();

        Data = tempConfig
            .AsEnumerable()
            .Where(x => x.Value is not null)
            .ToDictionary(
                x => x.Key,
                x => x.Value,
                StringComparer.OrdinalIgnoreCase);
    }
}
