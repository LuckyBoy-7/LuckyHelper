namespace LuckyHelper.Utils;

public static class LogUtils
{
    public static void LogInfo(string message)
    {
        Logger.Log(LogLevel.Info, "LuckyHelperTest", message);
    }

    public static void LogWarning(string message)
    {
        Logger.Log(LogLevel.Warn, "LuckyHelperTest", message);
    }
}