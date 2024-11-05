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
        On.Celeste.Level.End += LevelOnEnd;
    }

    // 这么写的问题就是，玩家在碰到trigger后然后死亡或者重试，状态仍然保留
    // 但感觉也不算一个大问题，毕竟本意就是开启这个状态，那只要在游戏里开了就行
    // 不然还要考虑是单房间保留状态还是全房间保留状态等等问题
    private static void LevelOnEnd(On.Celeste.Level.orig_End orig, Level self) // 防止本关的trigger在结束章节后还影响到其他地图
    {
        Active = false;
        orig(self);
    }

    private static void FallingBlockOnLandParticles(FallingBlock.orig_LandParticles orig, Celeste.FallingBlock self)
    {
        orig(self);
        if (!LuckyHelperModule.Settings.EnableFallingBlockBlockFloatySpaceBlock && !Active)
            return;
        foreach (var sceneEntity in self.SceneAs<Level>().Entities)
        {
            if (sceneEntity is FloatySpaceBlock)
            {
                for (float i = self.BottomLeft.X; i <= self.BottomRight.X - 8; i += 8)
                {
                    if (sceneEntity.CollidePoint(new Vector2(i, self.BottomLeft.Y)))
                    {
                        ((FloatySpaceBlock)typeof(FloatySpaceBlock).GetField("master", BindingFlags.NonPublic | BindingFlags.Instance)
                            ?.GetValue(sceneEntity))?.Moves.Clear();
                        ((FloatySpaceBlock)sceneEntity).Moves?.Clear();
                        break;
                    }
                }
            }
        }
    }

    [Unload]
    public static void Unload()
    {
        FallingBlock.LandParticles -= FallingBlockOnLandParticles;
        On.Celeste.Level.End -= LevelOnEnd;
    }
}