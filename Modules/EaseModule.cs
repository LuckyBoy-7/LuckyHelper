using System.Reflection;
using LuckyHelper.Entities.Misc;
using LuckyHelper.Module;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Level = On.Celeste.Level;

namespace LuckyHelper.Modules;

public class EaseModule
{
    // copied from https://github.com/lilybeevee/eevee-helper/blob/1869e253c9a35b4b226d7c8d00c2ad4b5e0a609e/Code/EeveeHelperModule.cs#L76
    public static Dictionary<string, Ease.Easer> EaseTypes = new Dictionary<string, Ease.Easer>
    {
        { "Linear", Ease.Linear },
        { "SineIn", Ease.SineIn },
        { "SineOut", Ease.SineOut },
        { "SineInOut", Ease.SineInOut },
        { "QuadIn", Ease.QuadIn },
        { "QuadOut", Ease.QuadOut },
        { "QuadInOut", Ease.QuadInOut },
        { "CubeIn", Ease.CubeIn },
        { "CubeOut", Ease.CubeOut },
        { "CubeInOut", Ease.CubeInOut },
        { "QuintIn", Ease.QuintIn },
        { "QuintOut", Ease.QuintOut },
        { "QuintInOut", Ease.QuintInOut },
        { "BackIn", Ease.BackIn },
        { "BackOut", Ease.BackOut },
        { "BackInOut", Ease.BackInOut },
        { "ExpoIn", Ease.ExpoIn },
        { "ExpoOut", Ease.ExpoOut },
        { "ExpoInOut", Ease.ExpoInOut },
        { "BigBackIn", Ease.BigBackIn },
        { "BigBackOut", Ease.BigBackOut },
        { "BigBackInOut", Ease.BigBackInOut },
        { "ElasticIn", Ease.ElasticIn },
        { "ElasticOut", Ease.ElasticOut },
        { "ElasticInOut", Ease.ElasticInOut },
        { "BounceIn", Ease.BounceIn },
        { "BounceOut", Ease.BounceOut },
        { "BounceInOut", Ease.BounceInOut }
    };
}