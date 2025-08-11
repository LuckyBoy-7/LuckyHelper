using LuckyHelper.Entities;
using LuckyHelper.Module;
using LuckyHelper.Triggers;
using Level = On.Celeste.Level;
using Player = On.Celeste.Player;

namespace LuckyHelper.Modules;

public class GhostTransposeModule
{
    public static bool EnableGhostTranspose;
    public static GhostTransposeTrigger.GhostOutOfBoundsActions GhostOutOfBoundsAction;
    public static GhostTransposeTrigger.TransposeDirTypes TransposeDirType;
    public static float GhostSpeed;
    public static bool UseDashKey;
    public static Color Color;
    public static float Alpha;
    public static float MaxGhostNumber;
    public static bool KillPlayerOnTeleportToSpike;
    public static bool ConserveMomentum;

    [Load]
    public static void Load()
    {
        On.Celeste.Player.Update += PlayerOnUpdate;
        On.Celeste.Level.LoadLevel += LevelOnLoadLevel;
        // var data = DynamicData.For(typeof(Player));
        // data.Set("EnableGhostTranspose", false);
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Player.Update -= PlayerOnUpdate;
        On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;
    }


    private static void PlayerOnUpdate(Player.orig_Update orig, Celeste.Player self)
    {
        // 没开启功能或是ghost个数达到上限了或者在barrier里
        self.Scene.Tracker.GetEntities<GhostTransposeBarrier>().ForEach(b => b.Collidable = true);
        bool inBarrier = self.CollideCheck<GhostTransposeBarrier>();
        self.Scene.Tracker.GetEntities<GhostTransposeBarrier>().ForEach(b => b.Collidable = false);
        if (!EnableGhostTranspose || Engine.Scene.Tracker.GetEntities<GhostTranspose>().Count >= MaxGhostNumber || inBarrier)
        {
            orig(self);
            return;
        }

        // Logger.Log(LogLevel.Warn, "Test", self.Sprite.HairOffset.ToString());
        bool ok = false;
        if (UseDashKey && Input.Dash.Pressed)
        {
            Input.Dash.ConsumeBuffer();
            ok = true;
        }
        else if (!UseDashKey && LuckyHelperModule.Settings.GhostTransposeButton.Pressed)
        {
            LuckyHelperModule.Settings.GhostTransposeButton.ConsumeBuffer();
            ok = true;
        }

        if (ok)
        {
            GhostTranspose ghost = new GhostTranspose(self, GhostOutOfBoundsAction, GhostSpeed, Alpha, Color, TransposeDirType, KillPlayerOnTeleportToSpike, ConserveMomentum);
            self.Scene.Add(ghost);
        }

        orig(self);
    }

    private static void LevelOnLoadLevel(Level.orig_LoadLevel orig, Celeste.Level self, Celeste.Player.IntroTypes playerintro, bool isfromloader)
    {
        EnableGhostTranspose = false;
        orig(self, playerintro, isfromloader);
    }
}