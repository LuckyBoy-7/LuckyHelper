using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;
using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Triggers;
using MonoMod.Utils;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/GhostTranspose")]
[Tracked]
public class GhostTranspose : Actor
{
    private GhostTransposeTrigger.GhostOutOfBoundsActions action;
    private GhostTransposeTrigger.TransposeDirTypes dirType;
    private float speed;
    private Vector2 dir;
    private Player player;
    private LuckyTrailManager.Snapshot Snapshot;
    private float alpha;
    private Color color;
    private bool killPlayerOnTeleportToSpike;

    public GhostTranspose(Player player, GhostTransposeTrigger.GhostOutOfBoundsActions action, float speed, float alpha, Color color,
        GhostTransposeTrigger.TransposeDirTypes dirType, bool killPlayerOnTeleportToSpike) : base(Vector2.Zero)
    {
        Depth = -1;
        // Depth = 1;
        this.player = player;
        this.dirType = dirType;
        this.action = action;
        this.speed = speed;
        this.alpha = alpha;
        this.color = color;
        this.killPlayerOnTeleportToSpike = killPlayerOnTeleportToSpike;

        Position = player.Position;
        Hitbox hitbox = new(player.Collider.Width, player.Collider.Height, player.Collider.TopLeft.X, player.Collider.TopLeft.Y);
        Collider = hitbox;

        switch (dirType)
        {
            case GhostTransposeTrigger.TransposeDirTypes.TwoSides:
                dir.X = Input.MoveX.Value;
                if (dir.X == 0)
                    dir.X = (int)this.player.Facing;
                break;
            case GhostTransposeTrigger.TransposeDirTypes.EightSides:
                dir = DynamicData.For(this.player).Get<Vector2>("lastAim");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dirType), dirType, null);
        }


        Snapshot = LuckyTrailManager.Add(player, color * this.alpha, 999);
        Snapshot.Speed = Vector2.One * speed * dir;
    }

    public override void Update()
    {
        base.Update();
        Scene.Tracker.GetEntities<GhostTransposeBarrier>().ForEach(b => b.Collidable = true);
        bool collide = MoveH(dir.X * speed * Engine.DeltaTime) || MoveV(dir.Y * speed * Engine.DeltaTime);
        Scene.Tracker.GetEntities<GhostTransposeBarrier>().ForEach(b => b.Collidable = false);
        if (collide) // 如果撞到solid了
        {
            // Logger.Log(LogLevel.Warn, "Test", "123");
            OnCollide();
            TrySpikeCollisionCheck();
            return;
        }

        Level level = SceneAs<Level>();
        // 出界
        // Logger.Log(LogLevel.Warn, "Test", action.ToString());
        if (!level.IsInBounds(this))
        {
            switch (action)
            {
                case GhostTransposeTrigger.GhostOutOfBoundsActions.None:
                    break;
                case GhostTransposeTrigger.GhostOutOfBoundsActions.TreatAsSolid:
                    var bounds = level.Bounds;
                    Left = Math.Max(Left, bounds.Left);
                    Right = Math.Min(Right, bounds.Right);
                    Top = Math.Max(Top, bounds.Top);
                    Bottom = Math.Min(Bottom, bounds.Bottom);
                    OnCollide();
                    TrySpikeCollisionCheck();
                    break;
                case GhostTransposeTrigger.GhostOutOfBoundsActions.KillPlayer:
                    player?.Die(Vector2.Zero);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Kill();
        }
    }

    private void OnCollide()
    {
        player.Position = Position;
        player.Speed = Vector2.Zero;
        Kill();
    }

    private void TrySpikeCollisionCheck()
    {
        if (player == null || !killPlayerOnTeleportToSpike)
            return;
        if (player.CollideCheck<Spikes>())
            player.Die(Vector2.Zero);
    }

    public void Kill()
    {
        Snapshot.RemoveSelf();
        RemoveSelf();
    }
}