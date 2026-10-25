using Celeste.Mod.Entities;
using LuckyHelper.Module;
using LuckyHelper.Utils;
using Microsoft.Xna.Framework.Input;
using MonoMod.Cil;
using Glider = On.Celeste.Glider;
using Player = On.Celeste.Player;

namespace LuckyHelper.Entities.Misc;

[CustomEntity("LuckyHelper/ActorPushedBehaviorController")]
[Tracked]
public class ActorPushedBehaviorController : Entity
{
    private List<string> affectedActors;

    private bool allowPushed;
    private bool pushedOut;
    private bool onlyActiveInSolid;
    private string activeFlag;

    public ActorPushedBehaviorController(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        affectedActors = data.ParseToStringList("affectedActors");
        allowPushed = data.Bool("allowPushed");
        pushedOut = data.Bool("pushedOut");
        onlyActiveInSolid = data.Bool("onlyActiveInSolid");
        activeFlag = data.Attr("activeFlag");
    }

    [Load]
    public static void Load()
    {
        IL.Celeste.Solid.MoveHExact += SolidOnMoveHExact;
        IL.Celeste.Solid.MoveVExact += SolidOnMoveVExact;
    }


    [Unload]
    public static void Unload()
    {
        IL.Celeste.Solid.MoveHExact -= SolidOnMoveHExact;
        IL.Celeste.Solid.MoveVExact -= SolidOnMoveVExact;
    }

    private static bool CheckActive(Entity entity, string flag) => string.IsNullOrEmpty(flag) || entity.SceneAs<Level>().Session.GetFlag(flag);

    private static void SolidOnMoveHExact(ILContext il)
    {
        HookMoveHVExact(il, 4, true);
    }

    private static void SolidOnMoveVExact(ILContext il)
    {
        HookMoveHVExact(il, 3, false);
    }

    private static void HookMoveHVExact(ILContext il, int actorLoc, bool horizontal)
    {
        void MoveBack(Solid solid, int move)
        {
            if (horizontal)
                solid.X -= move;
            else
                solid.Y -= move;
        }

        void MoveFront(Solid solid, int move)
        {
            if (horizontal)
                solid.X += move;
            else
                solid.Y += move;
        }

        ILCursor cursor = new ILCursor(il);
        if (cursor.TryGotoNext(
                ins => ins.MatchLdloc(actorLoc),
                ins => ins.MatchLdfld(typeof(Actor).GetField("AllowPushing"))
            ))
        {
            cursor.Index += 2;
            cursor.EmitLdarg0(); // solid
            cursor.EmitLdarg1(); // move
            cursor.EmitLdloc(actorLoc); // actor
            cursor.EmitDelegate<Func<bool, Solid, int, Actor, bool>>((allowPushing, solid, move, actor) =>
            {
                var controllers = actor.SceneAs<Level>().Tracker.GetEntities<ActorPushedBehaviorController>();
                foreach (ActorPushedBehaviorController controller in controllers)
                {
                    if (!controller.affectedActors.Contains(actor.GetType().Name))
                        continue;
                    if (!CheckActive(actor, controller.activeFlag))
                        return allowPushing;
                    if (controller.onlyActiveInSolid)
                    {
                        // 先在里面后不在里面的情况
                        if (!actor.CollideCheck(solid))
                            return allowPushing;
                        // 后在里面先不在里面的情况
                        MoveBack(solid, move);
                        if (!actor.CollideCheck(solid))
                        {
                            MoveFront(solid, move);
                            return allowPushing;
                        }

                        MoveFront(solid, move);
                    }


                    return controller.allowPushed;
                }

                return allowPushing;
            });
        }

        if (cursor.TryGotoNext(
                ins => ins.MatchLdloc(actorLoc),
                ins => ins.MatchLdfld(typeof(Actor).GetField("TreatNaive"))
            ))
        {
            cursor.Index += 2;
            cursor.EmitLdarg0(); // solid
            cursor.EmitLdarg1(); // move
            cursor.EmitLdloc(actorLoc); // actor
            cursor.EmitDelegate<Func<bool, Solid, int, Actor, bool>>((treatNaive, solid, move, actor) =>
            {
                var controllers = actor.SceneAs<Level>().Tracker.GetEntities<ActorPushedBehaviorController>();
                foreach (ActorPushedBehaviorController controller in controllers)
                {
                    if (!controller.affectedActors.Contains(actor.GetType().Name))
                        continue;

                    if (!CheckActive(actor, controller.activeFlag))
                        return treatNaive;
                    if (controller.onlyActiveInSolid)
                    {
                        if (!actor.CollideCheck(solid))
                            return treatNaive;
                        MoveBack(solid, move);
                        if (!actor.CollideCheck(solid))
                        {
                            MoveFront(solid, move);
                            return treatNaive;
                        }

                        MoveFront(solid, move);
                    }

                    return !controller.pushedOut;
                }

                return treatNaive;
            });
        }
    }
}