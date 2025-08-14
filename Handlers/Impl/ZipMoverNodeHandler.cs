using System.Reflection;
using LuckyHelper.Components;
using LuckyHelper.Components.EeveeLike;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace LuckyHelper.Handlers.Impl;

public class ZipMoverNodeHandler : EntityHandler, IMoveable, IAnchorProvider
{
    private ZipMover zipMover;
    private bool first;

    public ZipMoverNodeHandler(Entity entity, bool first) : base(entity)
    {
        zipMover = entity as ZipMover;
        this.first = first; // start 节点是否在 container 内, 因为 renderer 位置未知, 所以分别写了两个 handler 来包含 contain start 和 contain target 的情况

        // 目前有点没看明白, 因为跟 move 的时候就已经会更改参数了
        if (first)
        {
            DynamicData.For(zipMover).Set("LuckyHelper_zipMoverNodeHandled", true);
        }
    }

    public override void OnAddPersistentSingletonComponent()
    {
        base.OnAddPersistentSingletonComponent();
        zipMover.pathRenderer?.AddNoDuplicatedComponent(new PersistentSingletonComponent(true));
    }

    public override void OnRemovePersistentSingletonComponent()
    {
        base.OnRemovePersistentSingletonComponent();
        zipMover.pathRenderer?.Get<PersistentSingletonComponent>()?.RemoveSelf();
    }

    public override void Destroy()
    {
        base.Destroy();
        zipMover.pathRenderer?.RemoveSelf();
    }

    public override int GetHashCoe()
    {
        return HashCode.Combine(first);
    }

    public override bool IsInside(EntityContainer container)
    {
        return InsideCheck(container, first, zipMover);
    }

    public override Rectangle GetBounds()
    {
        var pos = first ? zipMover.start : zipMover.target;
        return new Rectangle((int)pos.X, (int)pos.Y, 0, 0);
    }

    public bool Move(Vector2 move, Vector2? liftSpeed)
    {
        if (first)
        {
            zipMover.start += move;
        }
        else
        {
            zipMover.target += move;
        }

        var newPos = Vector2.Lerp(zipMover.start, zipMover.target, zipMover.percent);
        zipMover.MoveTo(newPos, zipMover.LiftSpeed);

        if (zipMover.pathRenderer != null)
        {
            UpdatePathRenderer(zipMover.start, zipMover.target);
        }

        return true;
    }

    private void UpdatePathRenderer(Vector2 newFrom, Vector2 newTo)
    {
        var pathRenderer = zipMover.pathRenderer;

        var from = newFrom + new Vector2(Entity.Width / 2f, Entity.Height / 2f);
        var to = newTo + new Vector2(Entity.Width / 2f, Entity.Height / 2f);
        var angle = (from - to).Angle();

        pathRenderer.from = from;
        pathRenderer.to = to;
        pathRenderer.sparkAdd = (from - to).SafeNormalize(5f).Perpendicular();
        pathRenderer.sparkDirFromA = angle + ((float)Math.PI / 8f);
        pathRenderer.sparkDirFromB = angle - ((float)Math.PI / 8f);
        pathRenderer.sparkDirToA = angle + (float)Math.PI - ((float)Math.PI / 8f);
        pathRenderer.sparkDirToB = angle + (float)Math.PI + ((float)Math.PI / 8f);
    }

    public void PreMove()
    {
    }

    // Don't use default anchor handling
    public List<string> GetAnchors() => new();


    public static bool InsideCheck(EntityContainer container, bool first, ZipMover entity)
    {
        var pos = first ? entity.start : entity.target;
        return pos.X >= container.Entity.Left && pos.Y >= container.Entity.Top &&
               pos.X <= container.Entity.Right && pos.Y <= container.Entity.Bottom;
    }

    private static ILHook zipMoverSequenceHook;

    [Load]
    public static void Load()
    {
        zipMoverSequenceHook = new ILHook(typeof(ZipMover).GetMethod("Sequence", BindingFlags.Instance | BindingFlags.NonPublic).GetStateMachineTarget(), ZipMover_Sequence);
    }


    [Unload]
    public static void Unload()
    {
        zipMoverSequenceHook?.Dispose();
        zipMoverSequenceHook = null;
    }

    // 因为对于后来 attach 上的节点, 它的 start target 更新已经滞后了, 所以要 hook 一下
    private static void ZipMover_Sequence(ILContext il)
    {
        var cursor = new ILCursor(il);

        var thisLoc = -1;
        FieldReference fieldRef = null;

        if (!cursor.TryGotoNext(MoveType.After,
                instr => instr.MatchLdloc(out thisLoc),
                instr => instr.MatchLdfld<Entity>("Position"),
                instr => instr.MatchStfld(out fieldRef)))
        {
            Logger.Log("EeveeHelper", $"Failed zip mover hook");
            return;
        }

        while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld(fieldRef)))
        {
            Logger.Log("EeveeHelper", $"Hooking zip mover start field at position {cursor.Index}");

            cursor.Emit(OpCodes.Ldloc, thisLoc);
            cursor.EmitDelegate<Func<Vector2, ZipMover, Vector2>>((start, entity) =>
            {
                var data = DynamicData.For(entity);
                if (data.Get<bool?>("LuckyHelper_zipMoverNodeHandled") == true)
                {
                    return entity.start;
                }

                return start;
            });
        }
    }
}