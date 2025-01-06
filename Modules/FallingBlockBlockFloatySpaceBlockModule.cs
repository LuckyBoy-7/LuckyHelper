using System.Collections;
using System.Reflection;
using LuckyHelper.Module;
using FallingBlock = On.Celeste.FallingBlock;

namespace LuckyHelper.Modules;

public static class FallingBlockBlockFloatySpaceBlockModule
{

    [Load]
    public static void Load()
    {
        FallingBlock.LandParticles += FallingBlockOnLandParticles;
    }


    [Unload]
    public static void Unload()
    {
        FallingBlock.LandParticles -= FallingBlockOnLandParticles;
    }


    private static void FallingBlockOnLandParticles(FallingBlock.orig_LandParticles orig, Celeste.FallingBlock self)
    {
        orig(self);
        if (!LuckyHelperModule.Session.EnableFallingBlockBlocksFloatySpaceBlock)
            return;
        FloatySpaceBlock floatySpaceBlock = self.CollideFirst<FloatySpaceBlock>(self.Position + Vector2.UnitY);
        floatySpaceBlock?.master?.Moves?.Clear();
        floatySpaceBlock?.Moves?.Clear();
    }
}