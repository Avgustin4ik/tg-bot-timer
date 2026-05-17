namespace Console.Advanced.Services;

public static class CallbackData
{
    public static string Timer(string duration) => $"timer:{duration}";
    public static string TimerCustom() => "timer:custom";
    public static string DeleteTimer(long chatId) => $"delete:{chatId}";
    public static string ConfirmDelete(long chatId) => $"confirm_delete:{chatId}";
}