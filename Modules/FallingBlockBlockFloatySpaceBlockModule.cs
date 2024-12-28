using System.Collections;
using System.Reflection;
using LuckyHelper.Module;
using FallingBlock = On.Celeste.FallingBlock;

namespace LuckyHelper.Modules;

public static class FallingBlockBlockFloatySpaceBlockModule
{
    public static bool Active = false;

    [Load]
    public static void Load()
    {
        FallingBlock.LandParticles += FallingBlockOnLandParticles;
        On.Celeste.Level.LoadLevel += LevelOnLoadLevel;
    }


    [Unload]
    public static void Unload()
    {
        FallingBlock.LandParticles -= FallingBlockOnLandParticles;
        On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;
    }

    private static void LevelOnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Celeste.Level self, Celeste.Player.IntroTypes playerintro, bool isfromloader)
    {
        Active = false;
        orig(self, playerintro, isfromloader);
    }

    private static void FallingBlockOnLandParticles(FallingBlock.orig_LandParticles orig, Celeste.FallingBlock self)
    {
        orig(self);
        if (!Active)
            return;
        FloatySpaceBlock floatySpaceBlock = self.CollideFirst<FloatySpaceBlock>(self.Position + Vector2.UnitY);
        floatySpaceBlock?.master?.Moves?.Clear();
        floatySpaceBlock?.Moves?.Clear();
    }

}