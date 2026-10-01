using Celeste.Mod.Entities;
using Lucky.Kits.Collections;
using Lucky.Kits.Extensions;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using LuckyHelper.Triggers;
using LuckyHelper.Utils;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace LuckyHelper.Entities.Misc;

[CustomEntity("LuckyHelper/ShowCameraOffset")]
[Tracked]
public class ShowCameraOffset : Entity
{
    private bool show;

    public ShowCameraOffset(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        show = data.Bool("show");
        Depth = -100000000;
    }

    public override void Render()
    {
        base.Render();
        if (!show)
            return;

        Player player = Scene.Tracker.GetEntity<Player>();
        if (player == null)
            return;
        
        const int halfSize = 4;

        Level level = SceneAs<Level>();
        Vector2 offset = level.CameraOffset;
        Vector2 targetPosition = player.Position + offset;
        Draw.Rect(targetPosition.X - halfSize, targetPosition.Y - halfSize, halfSize * 2, halfSize * 2, Color.GreenYellow);
        
        // Vector2 cameraCenter = level.Camera.position + new Vector2(level.Camera.Viewport.Width, level.Camera.Viewport.Height) / 2;
        // Draw.Rect(cameraCenter.X - halfSize, cameraCenter.Y - halfSize, halfSize * 2, halfSize * 2, Color.Red);
    }
}