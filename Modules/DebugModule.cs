using LuckyHelper.Module;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Input;
using Player = Celeste.Player;

namespace LuckyHelper.Modules;

public static class DebugModule
{
    public static bool debug;

    [Command("luckyhelper_debug_on", "")]
    public static void DebugOn()
    {
        LogUtils.LogInfo("Debug On");
        debug = true;
    }

    [Command("luckyhelper_debug_off", "")]
    public static void DebugOff()
    {
        LogUtils.LogInfo("Debug Off");
        debug = false;
    }
}