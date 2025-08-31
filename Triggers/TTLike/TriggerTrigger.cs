using System.Collections;
using Celeste.Mod.Entities;
using LuckyHelper.Extensions;
using LuckyHelper.Module;
using LuckyHelper.Utils;

namespace LuckyHelper.Triggers.TTLike;

[CustomEntity("LuckyHelper/TriggerTrigger")]
[Tracked(false)]
public class TriggerTrigger : Trigger
{
    #region OnJumpHook

    public static OnJumpHandler OnJump = new();

    [Load]
    public static void Load()
    {
        On.Celeste.Player.Jump += PlayerOnJump;
        On.Celeste.Player.SuperJump += PlayerOnSuperJump;
        On.Celeste.Player.WallJump += PlayerOnWallJump;
        On.Celeste.Player.SuperWallJump += PlayerOnSuperWallJump;
    }


    [Unload]
    public static void Unload()
    {
        On.Celeste.Player.Jump -= PlayerOnJump;
        On.Celeste.Player.SuperJump -= PlayerOnSuperJump;
        On.Celeste.Player.WallJump -= PlayerOnWallJump;
        On.Celeste.Player.SuperWallJump -= PlayerOnSuperWallJump;
    }

    private static void PlayerOnWallJump(On.Celeste.Player.orig_WallJump orig, Player self, int dir)
    {
        OnJump.OnConditionFit(self);
        orig(self, dir);
    }

    private static void PlayerOnSuperJump(On.Celeste.Player.orig_SuperJump orig, Player self)
    {
        OnJump.OnConditionFit(self);
        orig(self);
    }

    private static void PlayerOnJump(On.Celeste.Player.orig_Jump orig, Player self, bool particles, bool playSfx)
    {
        OnJump.OnConditionFit(self);
        orig(self, particles, playSfx);
    }


    private static void PlayerOnSuperWallJump(On.Celeste.Player.orig_SuperWallJump orig, Player self, int dir)
    {
        OnJump.OnConditionFit(self);
        orig(self, dir);
    }


    public class OnJumpHandler
    {
        public void OnConditionFit(Player player)
        {
            foreach (TriggerTrigger tt in player.GetEntities<TriggerTrigger>())
            {
                if (tt.ActivationType == ActivationTypes.OnJump)
                {
                    if (!tt.CanAlwaysActivate)
                        tt.Add(new Coroutine(ActivateForOneFrame(tt)));
                }
            }
        }

        public IEnumerator ActivateForOneFrame(TriggerTrigger tt)
        {
            tt.CanAlwaysActivate = true;
            yield return null;
            tt.CanAlwaysActivate = false;
        }
    }

    #endregion

    public bool CoverRoom;
    public bool Activated;

    private Vector2[] nodes;
    private bool oneUse;
    public ActivationTypes ActivationType;
    public bool CanAlwaysActivate;
    private bool invertCondition;
    private float delay;
    private bool randomize;
    private bool matchPosition;
    private bool TryTriggerEveryFrame;

    private List<Trigger> triggers = new();
    private bool activating;
    private bool deactivating;
    private Trigger chosenTrigger;

    public enum ActivationTypes
    {
        OnJump,
    };


    public TriggerTrigger(EntityData data, Vector2 offset) : base(data, offset)
    {
        Depth = Depths.Top;
        nodes = data.NodesOffset(offset);
        oneUse = data.Bool("oneUse", false);
        ActivationType = data.Enum("activationType", ActivationTypes.OnJump);
        invertCondition = data.Bool("invertCondition", false);

        delay = data.Float("delay", 0f);
        randomize = data.Bool("randomize", false);
        CoverRoom = data.Bool("coverRoom", false);

        matchPosition = data.Bool("matchPosition", true);
        TryTriggerEveryFrame = data.Bool("tryTriggerEveryFrame", false);

        Add(new TransitionListener
        {
            OnOut = (f) => DeactivateTriggers(Scene?.Tracker.GetEntity<Player>())
        });
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        foreach (Vector2 node in nodes)
        {
            var origCollideable = new Dictionary<Trigger, bool>();
            foreach (Trigger trig in scene.Tracker.GetEntities<Trigger>())
            {
                origCollideable[trig] = trig.Collidable;
                trig.Collidable = true;
            }

            var trigger = scene.CollideFirst<Trigger>(node);

            foreach (Trigger trig in scene.Tracker.GetEntities<Trigger>())
            {
                trig.Collidable = origCollideable[trig];
            }

            if (trigger == null)
            {
                trigger = scene.Tracker.GetNearestEntity<Trigger>(node);
            }

            if (trigger != this && trigger != null)
            {
                triggers.Add(trigger);
                trigger.Collidable = false;
            }
        }
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);
        if (!TryTriggerEveryFrame)
        {
            TryActivate(player);
            if (Activated && oneUse)
            {
                RemoveSelf();
            }
        }
    }

    public override void OnLeave(Player player)
    {
        base.OnLeave(player);
        if (!TryTriggerEveryFrame)
        {
            TryDeactivate(player, false);
        }
    }

    public override void Update()
    {
        base.Update();

        var player = Scene.Tracker.GetEntity<Player>();

        if (player == null)
        {
            return;
        }

        if (TryTriggerEveryFrame)
        {
            if (CoverRoom || PlayerIsInside)
            {
                TryActivate(player);
                TryDeactivate(player, true);
            }
            else
            {
                TryDeactivate(player, false);
            }
        }

        if (Activated)
        {
            if (oneUse)
            {
                RemoveSelf();
            }
            else
            {
                UpdateTriggers(player);
            }
        }
    }

    private void UpdateTriggers(Player player)
    {
        CleanTriggers();

        foreach (Trigger trigger in triggers)
        {
            if (matchPosition)
            {
                MatchTriggerPosition(trigger);
            }

            if (!randomize)
            {
                trigger.OnStay(player);
            }
        }

        chosenTrigger?.OnStay(player);
    }

    private void CleanTriggers()
    {
        triggers.RemoveAll((trigger) => trigger.Scene == null);

        if (chosenTrigger?.Scene == null)
        {
            chosenTrigger = null;
        }
    }

    private void MatchTriggerPosition(Trigger trigger)
    {
        if (CoverRoom)
        {
            var level = SceneAs<Level>();
            trigger.Position = new Vector2(level.Bounds.X, level.Bounds.Y);
            trigger.Collider.Width = level.Bounds.Width;
            trigger.Collider.Height = level.Bounds.Height;
        }
        else
        {
            trigger.Position = Position;
            trigger.Collider.Width = Width;
            trigger.Collider.Height = Height;
        }
    }

    #region Activate Deactivate

    public void TryActivate(Player player)
    {
        if (activating || (Activated && !deactivating))
            return;

        if (GetActivateCondition(player))
        {
            if (delay > 0f)
            {
                activating = true;
                Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () =>
                {
                    activating = false;
                    ActivateTriggers(player);
                }, delay, true));
            }
            else
            {
                ActivateTriggers(player);
            }
        }
    }

    public void TryDeactivate(Player player, bool inside)
    {
        if (deactivating || (!Activated && !activating))
            return;

        if (!inside || !GetActivateCondition(player))
        {
            if (delay > 0f)
            {
                deactivating = true;
                Add(Alarm.Create(Alarm.AlarmMode.Oneshot, () =>
                {
                    deactivating = false;
                    DeactivateTriggers(player);
                }, delay, true));
            }
            else
            {
                DeactivateTriggers(player);
            }
        }
    }

    private void ActivateTriggers(Player player)
    {
        DeactivateTriggers(player);
        CleanTriggers();

        Activated = true;

        if (!randomize)
        {
            foreach (Trigger trigger in triggers)
            {
                if (trigger.PlayerIsInside)
                {
                    trigger.OnLeave(player);
                }

                trigger.OnEnter(player);
            }
        }
        else if (triggers.Count > 0)
        {
            Calc.PushRandom(GetSeed(SceneAs<Level>()));
            chosenTrigger = Calc.Choose(Calc.Random, triggers);
            Calc.PopRandom();

            if (chosenTrigger.PlayerIsInside)
            {
                chosenTrigger.OnLeave(player);
            }

            chosenTrigger.OnEnter(player);
        }
    }

    public static int GetSeed(Level level)
    {
        return (int)(level.Session.Time % int.MaxValue);
    }

    private void DeactivateTriggers(Player player)
    {
        CleanTriggers();

        Activated = false;

        foreach (Trigger trigger in triggers)
        {
            if (trigger.PlayerIsInside)
            {
                trigger.OnLeave(player);
            }
        }

        chosenTrigger = null;
    }

    public bool GetActivateCondition(Player player)
    {
        bool result = false;
        switch (ActivationType)
        {
            default:
                result = false;
                break;
        }

        if (CanAlwaysActivate)
        {
            result = true;
        }

        if (invertCondition)
        {
            result = !result;
        }

        return result;
    }

    #endregion
}