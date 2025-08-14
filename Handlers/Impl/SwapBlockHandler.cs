using System.Reflection;
using LuckyHelper.Components;
using LuckyHelper.Components.EeveeLike;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace LuckyHelper.Handlers.Impl;

public class SwapBlockHandler : EntityHandler, IMoveable, IAnchorProvider
{
    public const string HandledString = "LuckyHelper_SwapBlockHandled";

    private SwapBlock swapBlock;
    private bool first;

    public SwapBlockHandler(Entity entity, bool first) : base(entity)
    {
        swapBlock = entity as SwapBlock;
        this.first = first;

        DynamicData.For(swapBlock).Set(HandledString, true);
    }

    public override void OnAddPersistentSingletonComponent()
    {
        base.OnAddPersistentSingletonComponent();
        swapBlock.path?.AddNoDuplicatedComponent(new PersistentSingletonComponent(true));
    }

    public override void OnRemovePersistentSingletonComponent()
    {
        base.OnRemovePersistentSingletonComponent();
        swapBlock.path?.Get<PersistentSingletonComponent>()?.RemoveSelf();
    }

    public override void Destroy()
    {
        base.Destroy();
        swapBlock.path?.RemoveSelf();
    }

    public override int GetHashCoe()
    {
        return HashCode.Combine(first);
    }

    public override bool IsInside(EntityContainer container)
    {
        // LogUtils.LogDebug(InsideCheck(container, first, swapBlock).ToString());
        return InsideCheck(container, first, swapBlock);
    }

    public override Rectangle GetBounds()
    {
        var pos = first ? swapBlock.start : swapBlock.end;
        return new Rectangle((int)pos.X, (int)pos.Y, 0, 0);
    }

    public bool Move(Vector2 move, Vector2? liftSpeed)
    {
        if (first)
        {
            swapBlock.start += move;
        }
        else
        {
            swapBlock.end += move;
        }

        var newPos = Vector2.Lerp(swapBlock.start, swapBlock.end, swapBlock.lerp);
        swapBlock.MoveTo(newPos, swapBlock.LiftSpeed);

        int topleftX = (int)MathHelper.Min(swapBlock.start.X, swapBlock.end.X);
        int topleftY = (int)MathHelper.Min(swapBlock.start.Y, swapBlock.end.Y);
        int width = (int)MathHelper.Max(swapBlock.start.X + swapBlock.Width, swapBlock.end.X + swapBlock.Width);
        int height = (int)MathHelper.Max(swapBlock.start.Y + swapBlock.Height, swapBlock.end.Y + swapBlock.Height);
        swapBlock.moveRect = new Rectangle(topleftX, topleftY, width - topleftX, height - topleftY);

        return true;
    }

    public void PreMove()
    {
    }

    // Don't use default anchor handling
    public List<string> GetAnchors() => new();


    public static bool InsideCheck(EntityContainer container, bool first, SwapBlock entity)
    {
        var pos = first ? entity.start : entity.end;
        return pos.X >= container.Entity.Left && pos.Y >= container.Entity.Top &&
               pos.X <= container.Entity.Right && pos.Y <= container.Entity.Bottom;
    }

    [Load]
    public static void Load()
    {
        IL.Celeste.SwapBlock.Update += SwapBlock_Update;
        // On.Celeste.SwapBlock.PathRenderer.Render += PathRendererOnRender;
    }

    // private static void PathRendererOnRender(On.Celeste.SwapBlock.PathRenderer.orig_Render orig, Entity self)
    // {
    //     Draw.Rect(((SwapBlock.PathRenderer)self).block.moveRect, Color.Blue);
    //     orig(self);
    // }


    [Unload]
    public static void Unload()
    {
        IL.Celeste.SwapBlock.Update -= SwapBlock_Update;
        // On.Celeste.SwapBlock.PathRenderer.Render -= PathRendererOnRender;
    }


    private static void SwapBlock_Update(ILContext il)
    {
        var cursor = new ILCursor(il);
        if (cursor.TryGotoNext(MoveType.After, i => i.MatchLdarg(0), i2 => i2.MatchLdfld<Entity>("Position"), i3 => i3.MatchCall<Vector2>("op_Inequality")))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Call, typeof(SwapBlockHandler).GetMethod("ModifiedSwapBlockCheckHandler", BindingFlags.Static | BindingFlags.NonPublic));
        }
    }

    private static bool ModifiedSwapBlockCheckHandler(bool @in, SwapBlock swap)
    {
        var dyn = DynamicData.For(swap);
        if (!dyn.Data.ContainsKey(HandledString))
        {
            return @in;
        }

        Audio.Position(swap.moveSfx, swap.Center);
        Audio.Position(swap.returnSfx, swap.Center);

        if (swap.lerp == swap.target)
        {
            if (swap.target == 0)
            {
                Audio.SetParameter(swap.returnSfx, "end", 1f);
                Audio.Play("event:/game/05_mirror_temple/swapblock_return_end", swap.Center);
            }
            else
            {
                Audio.Play("event:/game/05_mirror_temple/swapblock_move_end", swap.Center);
            }
        }

        return false;
    }
}