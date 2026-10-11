using System.Globalization;
using System.Text.Json;

namespace WeatherApp;

public sealed class WeatherService : IDisposable
{
    private const int Retries = 3;

    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("https://wttr.in/"),
        Timeout = TimeSpan.FromSeconds(30),
    };

    public WeatherService()
    {
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("curl/8.0");
    }

    public async Task<WeatherData> GetWeatherAsync(string city)
    {
        using var json = await FetchJsonAsync(city);
        var root = json.RootElement;

        var tempText = root.GetProperty("current_condition")[0].GetProperty("temp_C").GetString()
            ?? throw new InvalidDataException("в ответе нет temp_C");
        var country = root.GetProperty("nearest_area")[0].GetProperty("country")[0].GetProperty("value").GetString()
            ?? throw new InvalidDataException("в ответе нет страны");

        return new WeatherData(city, int.Parse(tempText, CultureInfo.InvariantCulture), country);
    }

    private async Task<JsonDocument> FetchJsonAsync(string city)
    {
        var url = $"{Uri.EscapeDataString(city)}?format=j1";
        Exception? lastError = null;

        for (var attempt = 1; attempt <= Retries; attempt++)
        {
            try
            {
                using var response = await _http.GetAsync(url);
                response.EnsureSuccessStatusCode();
                await using var stream = await response.Content.ReadAsStreamAsync();
                return await JsonDocument.ParseAsync(stream);
            }
            catch (Exception error) when (error is HttpRequestException or TaskCanceledException or JsonException)
            {
                lastError = error;
                if (attempt < Retries)
                    await Task.Delay(TimeSpan.FromSeconds(attempt));
            }
        }

        throw new InvalidOperationException($"не удалось получить данные: {lastError?.Message}", lastError);
    }

    public void Dispose() => _http.Dispose();
}
