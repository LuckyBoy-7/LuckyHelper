using System.Reflection;
using LuckyHelper.Module;
using MonoMod.Cil;


namespace LuckyHelper.Modules;

public static class FastBubbleControllerModule
{
    [Load]
    public static void Load()
    {
        IL.Celeste.Player.BoostUpdate += PlayerOnBoostUpdate;
    }

    private static void PlayerOnBoostUpdate(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        MethodInfo moveToYMethod = typeof(Actor).GetMethod("MoveToY", BindingFlags.Instance | BindingFlags.Public);
        if (cursor.TryGotoNext(
                ins => ins.MatchCall(moveToYMethod)
        ))
        {
            cursor.Index += 1;
            ILLabel label = cursor.DefineLabel();
            
            cursor.EmitDelegate(() => LuckyHelperModule.Session.DisableFastBubble ? 1 : 0);
            cursor.EmitBrfalse(label);
            cursor.EmitLdcI4(Player.StBoost);
            cursor.EmitRet();
            
            cursor.MarkLabel(label);
        }
    }

    [Unload]
    public static void Unload()
    {
        IL.Celeste.Player.BoostUpdate -= PlayerOnBoostUpdate;
    }
}