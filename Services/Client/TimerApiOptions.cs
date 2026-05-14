namespace tg_bot_timer;

public record TimerApiOptions
{
    //just for dev
    public string BaseUrl { get; set; } = string.Empty;

    public string Create { get; set; } = "api/timers";
    public string List   { get; set; } = "api/timers";
    public string Get    { get; set; } = "api/timers/{0}";
    public string Update { get; set; } = "api/timers/{0}";
    public string Delete { get; set; } = "api/timers/{0}";
}