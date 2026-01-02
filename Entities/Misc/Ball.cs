using System.Collections;
using Celeste.Mod.CavernHelper;
using Celeste.Mod.Entities;
using Celeste.Mod.VortexHelper.Entities;
using LuckyHelper.Components.EeveeLike;
using LuckyHelper.Extensions;
using LuckyHelper.Modules;
using LuckyHelper.Utils;

namespace LuckyHelper.Entities.Misc;

[Tracked]
[CustomEntity("LuckyHelper/Ball")]
public class Ball : Actor
{
    private bool holdable;

    // gravity
    private float gravity;
    private bool HasGravity => gravity == 0 && (!waitForGrab || hasGrabbedOnce);
    private bool noDuplicateSelf;
    private bool noDuplicateOthers;
    private bool destroyable;
    private bool tutorial;
    private bool respawn;
    private bool waitForGrab;

    public EntityID ID;
    public Vector2 Speed;
    public Holdable Hold;
    private Utils.Timer noGravityTimer;
    private Utils.Timer highFrictionTimer;
    private bool destroyed;
    private bool slowFall;
    private Vector2 prevLiftSpeed;
    private Vector2 respawnPosition;

    // 教程相关
    private bool hasGrabbedOnce;
    private BirdTutorialGui tutorialGui;

    // sprite
    private Sprite ballSprite;

    // collider
    private int colliderSize;

    // speed modifier
    private float bounceSpeedMultiplierX;
    private float bounceSpeedMultiplierY;

    private float decelerationMultiplierX;
    private float decelerationMultiplierY;

    private float decelerationOnIceMultiplierX;

    private float DecelerationMultiplierX
    {
        get
        {
            float deceleration = decelerationMultiplierX;
            if (ModCompatModule.CavernHelperLoaded)
                deceleration = GetDecelerationMultiplierXWhenCavernLoaded(deceleration);
            if (ModCompatModule.VortexHelperLoaded)
                deceleration = GetDecelerationMultiplierXWhenVortexLoaded(deceleration);

            return deceleration;
        }
    }

    // rotate
    private float rotateAnimationSpeed;
    private float rotationRadians = 0;

    // audio
    private string hitSideSound;
    private string hitGroundSound;


    public Ball(EntityData data, Vector2 offset, EntityID id) : base(data.Position + offset + new Vector2(data.Width, data.Height) / 2f)
    {
        holdable = data.Bool("holdable");
        noDuplicateSelf = data.Bool("noDuplicateSelf");
        noDuplicateOthers = data.Bool("noDuplicateOthers");
        destroyable = data.Bool("destroyable", true);
        respawn = data.Bool("respawn");
        slowFall = data.Bool("slowFall");
        tutorial = data.Bool("tutorial");
        waitForGrab = data.Bool("waitForGrab");

        colliderSize = data.Int("colliderSize");
        ballSprite = new Sprite(GFX.Game, data.Attr("spritePath"));
        ballSprite.AddLoop("idle", "", 0.08f);
        ballSprite.Play("idle");
        ballSprite.CenterOrigin();
        ballSprite.Position.Y = -colliderSize / 2f;
        Add(ballSprite);

        ID = id;
        Collider = new Hitbox(colliderSize, colliderSize);
        Collider.Position = new Vector2(-colliderSize / 2f, -colliderSize);

        gravity = data.Float("gravity");
        AllowPushing = HasGravity;
        Depth = 100;
        respawnPosition = Position;

        bounceSpeedMultiplierX = data.Float("bounceSpeedMultiplierX");
        bounceSpeedMultiplierY = data.Float("bounceSpeedMultiplierY");
        decelerationMultiplierX = data.Float("decelerationMultiplierX");
        decelerationMultiplierY = data.Float("decelerationMultiplierY");
        decelerationOnIceMultiplierX = data.Float("decelerationOnIceMultiplierX");

        rotateAnimationSpeed = data.Float("rotateAnimationSpeed");

        hitSideSound = data.Attr("hitSideSound");
        hitGroundSound = data.Attr("hitGroundSound");


        if (holdable)
        {
            Add(Hold = new Holdable()
            {
                PickupCollider = new Hitbox(colliderSize + 8f, colliderSize + 8f, -colliderSize / 2f - 4f, -colliderSize - 4f),
                SlowFall = slowFall,
                SlowRun = data.Bool("slowRun", true),
                OnPickup = OnPickup,
                OnRelease = OnRelease,
                OnHitSpring = HitSpring,
                SpeedGetter = () => Speed,
                SpeedSetter = speed => Speed = speed,
                OnCarry = OnCarry
            });
        }

        SquishCallback = TryBigSquishWiggle;

        highFrictionTimer = new Utils.Timer(0.5f);
        noGravityTimer = new Utils.Timer(0.15f);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        foreach (Ball otherBall in scene.Tracker.GetEntities<Ball>())
        {
            if (otherBall != this && otherBall.Hold is { IsHeld: true })
            {
                // 只加载一个球
                if (otherBall.noDuplicateOthers)
                {
                    RemoveSelf();
                    return;
                }

                // 不重复加载自己
                if (otherBall.noDuplicateSelf && otherBall.ID.Key == ID.Key)
                {
                    RemoveSelf();
                    return;
                }
            }
        }

        if (tutorial)
        {
            tutorialGui = new BirdTutorialGui(this, new Vector2(0f, -colliderSize), Dialog.Clean("tutorial_carry", null), new object[]
            {
                Dialog.Clean("tutorial_hold", null),
                BirdTutorialGui.ButtonPrompt.Grab
            });
            tutorialGui.Open = true;
            scene.Add(tutorialGui);
        }
    }

    private void OnPickup()
    {
        hasGrabbedOnce = true;
        Speed = Vector2.Zero;
        AddTag(Tags.Persistent);
        highFrictionTimer.Restart();
    }

    private void OnRelease(Vector2 force)
    {
        RemoveTag(Tags.Persistent);

        if (slowFall)
        {
            force.Y *= 0.5f;
        }

        if (force.X != 0f && force.Y == 0f)
        {
            force.Y = -0.4f;
        }

        Speed = force * (slowFall ? 100f : 200f);
        if (Speed != Vector2.Zero)
        {
            noGravityTimer.Restart(0.1f);
        }
    }

    private void OnCarry(Vector2 target)
    {
        Position = target;
    }

    #region Wiggle

    private void TryBigSquishWiggle(CollisionData data)
    {
        data.Pusher.Collidable = true;
        for (var i = 0; i <= Math.Max(3, (int)colliderSize / 2); i++)
        {
            for (var j = 0; j <= Math.Max(3, (int)colliderSize / 2); j++)
            {
                if (i != 0 || j != 0)
                {
                    for (var k = 1; k >= -1; k -= 2)
                    {
                        for (var l = 1; l >= -1; l -= 2)
                        {
                            var value = new Vector2(i * k, j * l);
                            if (!CollideCheck<Solid>(Position + value))
                            {
                                Position += value;
                                data.Pusher.Collidable = false;
                                return;
                            }
                        }
                    }
                }
            }
        }

        data.Pusher.Collidable = false;
    }

    #endregion

    public override void Update()
    {
        base.Update();

        Collidable = !destroyed;

        TryDropBallOnUncollidable();


        if (!destroyed)
        {
            if (destroyable)
            {
                if (TryDestroyBall())
                    return;
            }

            if (tutorial)
            {
                tutorialGui.Open = !hasGrabbedOnce;
            }
        }


        if (holdable && Hold.IsHeld)
        {
            prevLiftSpeed = Vector2.Zero;
        }
        else if (!UpdateOnNotHeld())
            return;

        if (TryDestroySelfAfterMoveToRightRoom())
            return;

        Hold?.CheckAgainstColliders();

        UpdateRotation();
    }

    private void UpdateRotation()
    {
        if (rotateAnimationSpeed != 0f)
        {
            float rotationSpeed = Speed.X * rotateAnimationSpeed / 20;
            rotationRadians += rotationSpeed * Engine.DeltaTime;
            ballSprite.Rotation = rotationRadians;
        }
    }

    private bool TryDestroySelfAfterMoveToRightRoom()
    {
        Level level = this.Level();
        if (Center.X > level.Bounds.Right)
        {
            MoveH(32f * Engine.DeltaTime);
            if (Left - colliderSize / 2f > level.Bounds.Right)
            {
                RemoveSelf();
                return true;
            }
        }

        return false;
    }

    private bool UpdateOnNotHeld()
    {
        highFrictionTimer.Update(Engine.DeltaTime);

        var level = SceneAs<Level>();
        if (TryUpdateSpringHitWhenCantHold())
        {
        }
        else if (OnGround(1))
        {
            TrySlipOffCliff();
            UpdateLiftBoost();
        }
        else if (!holdable || Hold.ShouldHaveGravity)
        {
            UpdateSpeedInAir();
        }

        MoveH(Speed.X * Engine.DeltaTime, OnCollideH);
        MoveV(Speed.Y * Engine.DeltaTime, OnCollideV);

        // 只能扔出右边
        if (Left < level.Bounds.Left)
        {
            Left = level.Bounds.Left;
            Speed.X *= -0.4f;
        }
        else if (Top < level.Bounds.Top - 4)
        {
            Top = level.Bounds.Top + 4;
            Speed.Y = 0f;
        }
        else if (Top > level.Bounds.Bottom)
        {
            if (!respawn)
            {
                RemoveSelf();
            }
            else if (!destroyed)
            {
                Add(new Coroutine(DestroyRoutine()));
            }

            return false;
        }

        return true;
    }

    private void UpdateSpeedInAir()
    {
        float xAccel;
        float yAccel;
        if (!slowFall)
        {
            yAccel = 800f;
            if (Math.Abs(Speed.Y) <= 30f)
            {
                yAccel *= 0.5f;
            }

            xAccel = 350f;
            if (Speed.Y < 0f)
            {
                xAccel *= 0.5f;
            }
        }
        else
        {
            yAccel = 200f;
            if (Speed.Y >= -30f)
            {
                yAccel *= 0.5f;
            }

            if (Speed.Y < 0f)
            {
                xAccel = 40f;
            }
            else if (highFrictionTimer.Over)
            {
                xAccel = 40f;
            }
            else
            {
                xAccel = 10f;
            }
        }

        xAccel *= DecelerationMultiplierX;
        yAccel *= decelerationMultiplierY;

        Speed.X = Calc.Approach(Speed.X, 0f, xAccel * Engine.DeltaTime);

        if (!HasGravity)
            return;
        noGravityTimer.Update(Engine.DeltaTime);
        if (noGravityTimer.Over)
        {
            Speed.Y = Calc.Approach(Speed.Y, slowFall ? 30f : 200f, yAccel * Engine.DeltaTime);
        }
    }

    private float GetDecelerationMultiplierXWhenCavernLoaded(float origDeceleration)
    {
        if (CollideCheck<IcyFloor>())
        {
            return decelerationOnIceMultiplierX;
        }

        return origDeceleration;
    }

    private float GetDecelerationMultiplierXWhenVortexLoaded(float origDeceleration)
    {
        if (CollideFirst<FloorBooster>() is { IceMode: true })
        {
            return decelerationOnIceMultiplierX;
        }

        return origDeceleration;
    }

    private void UpdateLiftBoost()
    {
        var liftSpeed = LiftSpeed;
        if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
        {
            Speed = prevLiftSpeed;
            prevLiftSpeed = Vector2.Zero;
            Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
            if (Speed.X != 0f && Speed.Y == 0f)
            {
                Speed.Y = -60f;
            }

            if (Speed.Y < 0f)
            {
                noGravityTimer.Restart(0.15f);
            }
        }
        else
        {
            prevLiftSpeed = liftSpeed;
            if (liftSpeed.Y < 0f && Speed.Y < 0f)
            {
                Speed.Y = 0f;
            }
        }
    }

    private void TrySlipOffCliff()
    {
        float target;
        if (!OnGround(Position + Vector2.UnitX * 3f, 1))
        {
            target = 20f;
        }
        else if (!OnGround(Position - Vector2.UnitX * 3f, 1))
        {
            target = -20f;
        }
        else
        {
            target = 0f;
        }

        float xAccel = 800f * DecelerationMultiplierX;
        Speed.X = Calc.Approach(Speed.X, target, xAccel * Engine.DeltaTime);
    }


    /// <summary>
    /// 当我们无法抓取 ball 的时候, 也就没有 holdable 在 spring 中的回调, 只能自己检测弹簧碰撞
    /// </summary>
    /// <returns></returns>
    private bool TryUpdateSpringHitWhenCantHold()
    {
        Spring spring = null;
        if (!holdable)
        {
            foreach (var otherEntity in Scene.Entities)
            {
                if (otherEntity is Spring currentSpring && CollideCheck(otherEntity))
                {
                    spring = currentSpring;
                    break;
                }
            }
        }

        if (spring != null)
        {
            HitSpring(spring);
            spring.BounceAnimate();
        }

        return false;
    }

    private bool TryDestroyBall()
    {
        foreach (SeekerBarrier barrier in Scene.Tracker.GetEntities<SeekerBarrier>())
        {
            var collided = this.SafeCollideCheck(barrier);
            if (collided)
            {
                if (holdable && Hold.IsHeld)
                    DropBall();

                Add(new Coroutine(DestroyRoutine()));
                return true;
            }
        }

        return false;
    }

    private void TryDropBallOnUncollidable()
    {
        if (!Collidable && holdable && Hold.IsHeld)
        {
            DropBall();
        }
    }

    private void DropBall()
    {
        var playerSpeed = Hold.Holder.Speed;
        Hold.Holder.Drop();
        Speed = playerSpeed * 0.5f;
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
    }

    public override bool IsRiding(Solid solid)
    {
        return HasGravity && base.IsRiding(solid);
    }

    public override bool IsRiding(JumpThru jumpThru)
    {
        return HasGravity && base.IsRiding(jumpThru);
    }

    private IEnumerator DestroyRoutine()
    {
        destroyed = true;
        Collidable = false;
        if (tutorialGui != null)
        {
            tutorialGui.Open = false;
        }

        Audio.Play(SFX.game_10_glider_emancipate, Position);
        var tween = Tween.Create(Tween.TweenMode.Oneshot, null, 0.2f, true);
        tween.OnUpdate = t => { ballSprite.Color = Color.Red * (1 - t.Eased); };
        Add(tween);
        yield return 0.2f;
        if (respawn)
        {
            hasGrabbedOnce = false;
            Speed = Vector2.Zero;
            Position = respawnPosition;
        }

        var tween2 = Tween.Create(Tween.TweenMode.Oneshot, null, 0.1f, true);
        tween2.OnUpdate = t => ballSprite.Color = Color.White * t.Eased;
        Add(tween2);
        yield return 0.1f;
        if (respawn)
        {
            destroyed = false;
            Collidable = true;
        }
        else
            RemoveSelf();
    }

    private void OnCollideH(CollisionData data)
    {
        if (data.Hit is DashSwitch dashSwitch)
        {
            dashSwitch.OnDashCollide(null, Vector2.UnitX * Math.Sign(Speed.X));
        }

        Speed.X *= bounceSpeedMultiplierX;
        Audio.Play(hitGroundSound, Position);
    }

    private void OnCollideV(CollisionData data)
    {
        if (data.Hit is DashSwitch dashSwitch)
        {
            dashSwitch.OnDashCollide(null, Vector2.UnitY * Math.Sign(Speed.Y));
        }

        // 防止反弹速度倍率太小的时候在地上抽搐
        if (Speed.Y is > 0 and < 60f)
            Speed.Y = 0;
        Speed.Y *= bounceSpeedMultiplierY;
        Audio.Play(hitSideSound, Position);
    }

    public bool HitSpring(Spring spring)
    {
        if (!holdable || !Hold.IsHeld)
        {
            if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
            {
                Speed.X *= 0.5f;
                Speed.Y = -160f;
                noGravityTimer.Restart(0.15f);
                return true;
            }

            if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
            {
                MoveTowardsY(spring.CenterY + 5f, 4f, null);
                Speed.X = slowFall ? 160f : 220f;
                Speed.Y = -80f;
                noGravityTimer.Restart(0.1f);
                return true;
            }

            if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
            {
                MoveTowardsY(spring.CenterY + 5f, 4f, null);
                Speed.X = slowFall ? -160f : -220f;
                Speed.Y = -80f;
                noGravityTimer.Restart(0.1f);
                return true;
            }
        }

        return false;
    }
}