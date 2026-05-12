namespace tg_bot_timer;

public record TimerResponse
(
    int Id,
    string Name,
    string Duration,
    DateTime ExpiresAt,
    bool IsActive = true
);