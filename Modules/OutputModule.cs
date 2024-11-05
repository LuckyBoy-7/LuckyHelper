using System.Reflection;
using Lucky.Collections;
using LuckyHelper.Module;
using Microsoft.Xna.Framework.Input;
using Engine = On.Monocle.Engine;


namespace LuckyHelper.Modules;

public class OutputModule
{
    public static Counter<Type> PreEntityTypeCounter = new();

    [Load]
    public static void Load()
    {
        On.Celeste.Celeste.Update += CelesteOnUpdate;
    }

    [Unload]
    public static void Unload()
    {
        On.Celeste.Celeste.Update -= CelesteOnUpdate;
    }

    private static void CelesteOnUpdate(On.Celeste.Celeste.orig_Update orig, Celeste.Celeste self, GameTime gametime)
    {
        orig(self, gametime);
        if (!LuckyHelperModule.Settings.IsDebugging)
            return;
        if (MInput.Keyboard.Pressed(Keys.Y))
            OutputCurrentEntitiyTypes(Celeste.Celeste.Scene);
        if (MInput.Keyboard.Pressed(Keys.U))
        {
            Logger.Log(LogLevel.Info, "Test", $"========================ClearPreEntityTypeCounter==================");
            PreEntityTypeCounter.Clear();
        }
    }


    private static void OutputCurrentEntitiyTypes(Scene scene)
    {
        // Logger.Log(LogLevel.Info, "Test", $"==============CurrentSceneName: {scene}=============");
        Logger.Log(LogLevel.Info, "Test", $"==============DiffEntitiyTypes=============");
        Counter<Type> counter = new(scene.Entities.Select(e => e.GetType()).GetEnumerator());
        Counter<Type> newCounter = counter - PreEntityTypeCounter;

        List<Type> sortedTypes = newCounter.Keys.ToList();
        sortedTypes.Sort((s1, s2) => string.Compare(s1.ToString(), s2.ToString()));
        foreach (var type in sortedTypes)
            Logger.Log(LogLevel.Info, "Test", $"{type} * {newCounter[type]}");
        PreEntityTypeCounter = counter;
    }
}