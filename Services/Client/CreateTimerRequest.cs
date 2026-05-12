namespace tg_bot_timer;

public record CreateTimerRequest
(
    string Duration,           // "12h15min5sec", "2h", "30m" и т.д.
    long ChatId,
    long? UserId = null,
    string? Name = null
);