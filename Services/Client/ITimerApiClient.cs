namespace tg_bot_timer;

public interface ITimerApiClient
{
    Task<TimerResponse?> CreateTimerAsync(CreateTimerRequest request);
    Task<List<TimerResponse>> GetTimersAsync(long? chatId = null);
    Task<TimerResponse?> GetTimerAsync(int id);
    Task<bool> UpdateTimerAsync(int id, CreateTimerRequest request);
    Task<bool> DeleteTimerAsync(int id);
}