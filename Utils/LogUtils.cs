namespace LuckyHelper.Utils;

public static class LogUtils
{
    public static void LogInfo(string message)
    {
        Logger.Log(LogLevel.Info, "LuckyHelper", message);
    }

    public static void LogWarning(string message)
    {
        Logger.Log(LogLevel.Warn, "LuckyHelper", message);
    }
    
    public static void LogDebug(string message)
    {
        Logger.Log(LogLevel.Debug, "LuckyHelper", message);
    }
}