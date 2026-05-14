namespace tg_bot_timer;

using System.Net.Http.Json;
using Microsoft.Extensions.Options;

public class TimerApiClient : ITimerApiClient
{
    private readonly HttpClient _httpClient;
    private readonly TimerApiOptions _options;
    private readonly ILogger<TimerApiClient> _logger;

    public TimerApiClient(HttpClient httpClient, IOptions<TimerApiOptions> options, ILogger<TimerApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<TimerResponse?> CreateTimerAsync(CreateTimerRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(_options.Create, request);
            response.EnsureSuccessStatusCode();

            var timer = await response.Content.ReadFromJsonAsync<TimerResponse>();
            _logger.LogInformation("Таймер создан. ID: {Id}, Duration: {Duration}", timer?.Id, request.Duration);
            return timer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании таймера {Duration}", request.Duration);
            return null;
        }
    }

    public async Task<List<TimerResponse>> GetTimersAsync(long? chatId = null)
    {
        var url = _options.List;
        if (chatId.HasValue)
            url += $"?chatId={chatId}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<TimerResponse>>() ?? [];
    }

    public async Task<TimerResponse?> GetTimerAsync(int id)
    {
        var url = string.Format(_options.Get, id);
        return await _httpClient.GetFromJsonAsync<TimerResponse>(url);
    }

    public async Task<bool> UpdateTimerAsync(int id, CreateTimerRequest request)
    {
        var url = string.Format(_options.Update, id);
        var response = await _httpClient.PutAsJsonAsync(url, request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteTimerAsync(int id)
    {
        var url = string.Format(_options.Delete, id);
        var response = await _httpClient.DeleteAsync(url);
        return response.IsSuccessStatusCode;
    }
}