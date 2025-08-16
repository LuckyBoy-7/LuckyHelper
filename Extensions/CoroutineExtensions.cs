namespace LuckyHelper.Extensions;

internal static class CoroutineExtensions
{
    public static void RunToEnd(this Coroutine coroutine)
    {
        while (!coroutine.Finished)
            coroutine.Update();
    }
}