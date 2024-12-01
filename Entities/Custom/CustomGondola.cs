using System.Collections;
using Celeste.Mod.Entities;
using MonoMod.Utils;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/CustomGondola")]
public class CustomGondola : Gondola
{
    private float k;
    private float curSpeed;
    private float a;
    private Vector2 curDir;
    private Vector2 startToEndDir;


    private Vector2 start;
    private Vector2 end;
    private Vector2 _realStart;

    private Vector2 RealStart => (start + new Vector2(startPositionOffsetX, startPositionOffsetX * k)).Floor();
    private Vector2 RealEnd => (end + new Vector2(endPositionOffsetX, endPositionOffsetX * k)).Floor();

    private float maxSpeed;
    private float accelerationMoveDist;

    private Coroutine coroutine;
    private Solid ceiling;
    private const int CeilingOffsetY = 17;
    private Hitbox ceilingHitbox;

    private enum StateTypes
    {
        MoveToStart = 0,
        MoveToEnd = 1,
        Idle = 2
    }

    private StateTypes state = StateTypes.Idle;
    private StateTypes preMoveState = StateTypes.MoveToStart;


    private SoundSource moveLoopSfx;
    private SoundSource startOffSoundSource;

    private TalkComponent interact;

    private Vector2 _targetPos;

    private Vector2 TargetPos
    {
        get => _targetPos.Floor();
        set => _targetPos = value;
    }

    private bool lockInteractable;

    // todo: 以下都是可供mapper调整的部分
    private bool canInteractOnMove = true;
    private bool addCeiling = true;

    private int startPositionOffsetX = 200;
    private int endPositionOffsetX = -200;
    private float rotationSpeed = 0.7f;

    private float accelerationDuration = 2; // 起步和减速的时间
    private float moveDuration = 6; // 中间段的移动时间, 因为加速是匀加速的所所以位移曲线是个等腰梯形, 面积为(accelerationDuration + moveDuration) * curSpeed = dist

    // private string frontTexturePath = "LuckyHelper/objects/gondola/front";
    // private string leverTexturePath = "LuckyHelper/objects/gondola/lever";
    // private string backTexturePath = "LuckyHelper/objects/gondola/back";
    // private string cliffsideLeftTexturePath = "LuckyHelper/objects/gondola/cliffsideLeft";
    // private string cliffsideRightTexturePath = "LuckyHelper/objects/gondola/cliffsideRight";
    // private string topTexturePath = "LuckyHelper/objects/gondola/top";
    private string frontTexturePath = "objects/gondola/front";
    private string leverTexturePath = "objects/gondola/lever";
    private string backTexturePath = "objects/gondola/back";
    private string cliffsideLeftTexturePath = "objects/gondola/cliffsideLeft";
    private string cliffsideRightTexturePath = "objects/gondola/cliffsideRight";
    private string topTexturePath = "objects/gondola/top";

    public enum StartPositionTypes
    {
        Start,
        End,
        CloseToPlayer
    }

    private StartPositionTypes startPositionType;

    public CustomGondola(EntityData data, Vector2 offset) : base(data, offset)
    {
        canInteractOnMove = data.Bool("canInteractOnMove");
        addCeiling = data.Bool("addCeiling");
        startPositionOffsetX = data.Int("startPositionOffsetX");
        endPositionOffsetX = data.Int("endPositionOffsetX");
        rotationSpeed = data.Float("rotationSpeed");
        accelerationDuration = data.Float("accelerationDuration");
        moveDuration = data.Float("moveDuration");

        frontTexturePath = data.Attr("frontTexturePath");
        leverTexturePath = data.Attr("leverTexturePath");
        backTexturePath = data.Attr("backTexturePath");
        cliffsideLeftTexturePath = data.Attr("cliffsideLeftTexturePath");
        cliffsideRightTexturePath = data.Attr("cliffsideRightTexturePath");
        topTexturePath = data.Attr("topTexturePath");
        
        startPositionType = data.Enum<StartPositionTypes>("startPositionType");


        start = Position;
        end = data.Nodes[0] + offset;

        coroutine = new Coroutine(false);
        Add(coroutine);

        if (addCeiling)
        {
            ceiling = new Solid(Position + new Vector2(-Collider.Width / 2, -CeilingOffsetY), Collider.Width, Collider.Height, true);
            ceilingHitbox = new Hitbox(Collider.Width, 4, -Collider.Width / 2, -CeilingOffsetY);
            ceiling.Collider = ceilingHitbox;
            ceiling.SurfaceSoundIndex = 28;
        }

        Add(moveLoopSfx = new SoundSource());
        startOffSoundSource = new SoundSource();
        Add(interact = new TalkComponent(new Rectangle(-24, -8, 48, 8), new Vector2(-0.5f, -20f), Interact));
    }

    private void Interact(Player obj)
    {
        ChangeState();
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);
        if (addCeiling)
            Scene.Add(ceiling);

        LeftCliffside.Position = start;
        RightCliffside.Position = end;

        // 对齐到缆车真正的位置
        start += new Vector2(34, -6) - new Vector2(0, -52);
        end += new Vector2(-35, 0) - new Vector2(0, -52);

        startToEndDir = (end - start).SafeNormalize();
        if (startToEndDir.X == 0)
            k = float.MaxValue * Single.Sign(startToEndDir.Y);
        else
            k = startToEndDir.Y / startToEndDir.X;

        // todo:到时候记得处理percent 距离
        float dist = (RealStart - RealEnd).Length();
        maxSpeed = dist / (accelerationDuration + moveDuration);
        accelerationMoveDist = maxSpeed * accelerationDuration / 2;
        a = maxSpeed / accelerationDuration;

        // 自动获取初始位置
        switch (startPositionType)
        {
            case StartPositionTypes.Start:
                Position = RealStart;
                preMoveState = StateTypes.MoveToStart;
                break;
            case StartPositionTypes.End:
                Position = RealEnd;
                preMoveState = StateTypes.MoveToEnd;
                break;
            case StartPositionTypes.CloseToPlayer:
                Logger.Log(LogLevel.Warn,"Test","123123");
                Vector2 pos = Scene.Tracker.GetEntity<Player>().Position;
                if (Vector2.Distance(end, pos) < Vector2.Distance(start, pos))
                {
                    Position = RealEnd;
                    preMoveState = StateTypes.MoveToEnd;
                }
                else
                {
                    Position = RealStart;
                    preMoveState = StateTypes.MoveToStart;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        // if (autoAlignToGoundPositionOfPlayer)
        // {
        //     Position = start;
        //     Player player = Scene.Tracker.GetEntity<Player>();
        //     Logger.Log(LogLevel.Warn, "Test", (player == null).ToString());
        //     bool hit = false;
        //     int hitY = -1;
        //     for (int i = 1; i < 200; i++)
        //     {
        //         if (player.CollideCheck<Solid>(player.Position + Vector2.UnitY * i))
        //         {
        //             hit = true;
        //             hitY = (int)player.Position.Y + i - 1;
        //             break;
        //         }
        //     }
        //
        //     if (!hit)
        //     {
        //         Position = RealStart;
        //     }
        //     else
        //     {
        //         curDir = (end - start).SafeNormalize();
        //         if (Single.Sign(Position.Y - hitY) == Single.Sign(curDir.Y))
        //             curDir *= -1;
        //
        //         while (hitY != Position.Y)
        //         {
        //             CustomMoveH(curDir.X * Engine.DeltaTime, false);
        //             CustomMoveV(curDir.Y * Engine.DeltaTime, false);
        //         }
        //
        //         _realStart = Position;
        //
        //         preMoveState = Vector2.Distance(end, Position) < Vector2.Distance(start, Position) ? StateTypes.MoveToEnd : StateTypes.MoveToStart;
        //     }
        // }
        // else if (autoAlignToStartPosition)
        // {
        //     Position = start;
        //     curDir = (end - start).SafeNormalize();
        //     while (LeftCliffside.Y != Position.Y)
        //     {
        //         CustomMoveH(curDir.X * Engine.DeltaTime, false);
        //         CustomMoveV(curDir.Y * Engine.DeltaTime, false);
        //     }
        //
        //     _realStart = Position;
        // }
        // else
        // {
        //     Position = RealStart;
        // }

        if (addCeiling)
        {
            ceiling.Position = Position + new Vector2(0, -CeilingOffsetY);
            // DynamicData thisData = DynamicData.For(this);
            // DynamicData otherData = DynamicData.For(ceiling);
            // otherData.Set("movementCounter", thisData.Get<Vector2>("movementCounter"));
        }

        DynamicData dd = DynamicData.For(this);

        // back
        Entity back = dd.Get<Entity>("back");
        back.Position = Position;
        Image backImage = dd.Get<Image>("backImg");
        backImage.Texture = GFX.Game[backTexturePath];

        // top
        Image top = dd.Get<Image>("top");
        top.Texture = GFX.Game[topTexturePath];

        // front
        Sprite front = dd.Get<Sprite>("front");
        front.Reset(GFX.Game, frontTexturePath);
        front.Add("idle", "", 0);
        front.Play("idle");
        // lever
        Lever.Reset(GFX.Game, leverTexturePath);
        Lever.Add("idle", "", 0f, new int[1]);
        Lever.Add("pulled", "", 0.5f, "idle", 1, 1);
        Lever.Play("idle");

        // cliffside
        LeftCliffside.Get<Image>().Texture = GFX.Game[cliffsideLeftTexturePath];
        RightCliffside.Get<Image>().Texture = GFX.Game[cliffsideRightTexturePath];
    }


    public override void Update()
    {
        base.Update();
        interact.Enabled = ((canInteractOnMove && curSpeed == maxSpeed) || curSpeed == 0) && !lockInteractable;
        // debug
        // if (MInput.Keyboard.Pressed(Keys.Space))
        // {
        //     ChangeState();
        // }


        moveLoopSfx.Position = Position;
        startOffSoundSource.Position = Position;
        // Logger.Log(LogLevel.Warn, "Test", start.ToString());
        // Logger.Log(LogLevel.Warn, "Test", Position.ToString());
        if (RotationSpeed == 0 && curSpeed == 0)
        {
            moveLoopSfx.Stop();
            startOffSoundSource.Stop();
        }

        if (Vector2.Distance(TargetPos, Position) < 5)
        {
            lockInteractable = true;
            curSpeed = 0;
            if (coroutine != null)
                coroutine.Cancel();
            Vector2 dir = (TargetPos - Position).SafeNormalize();
            CustomMoveH(dir.X);
            CustomMoveV(dir.Y);
            if (TargetPos == Position)
            {
                lockInteractable = false;
                state = StateTypes.Idle;
            }
        }
    }

    private void ChangeState()
    {
        switch (state)
        {
            case StateTypes.Idle:
                if (curSpeed == 0)
                {
                    startOffSoundSource.Play("event:/game/04_cliffside/gondola_cliffmechanism_start");
                    moveLoopSfx.Play("event:/game/04_cliffside/gondola_movement_loop");
                    Lever.Play("pulled", true);
                    if (preMoveState == StateTypes.MoveToEnd)
                    {
                        RotationSpeed = -rotationSpeed;
                        TargetPos = RealStart;
                        coroutine.Replace(MoveToTargetPosCoroutine());
                    }
                    else if (preMoveState == StateTypes.MoveToStart)
                    {
                        RotationSpeed = rotationSpeed;
                        TargetPos = RealEnd;
                        coroutine.Replace(MoveToTargetPosCoroutine());
                    }

                    preMoveState = (StateTypes)(((int)preMoveState + 1) % 2);
                    state = preMoveState;
                }

                break;
            case StateTypes.MoveToStart:
                if (curSpeed == maxSpeed)
                {
                    Lever.Play("pulled", true);
                    coroutine.Replace(SlowDown());
                }

                break;
            case StateTypes.MoveToEnd:
                if (curSpeed == maxSpeed)
                {
                    Lever.Play("pulled", true);
                    coroutine.Replace(SlowDown());
                }

                break;
        }
    }

    private IEnumerator SlowDown()
    {
        float elapse = 0;
        float startSpeed = curSpeed;
        float t = curSpeed / a;
        while (elapse != t)
        {
            elapse = Calc.Approach(elapse, t, Engine.DeltaTime);
            curSpeed = (1 - (elapse / t)) * startSpeed;
            CustomMoveH((curSpeed * curDir).X * Engine.DeltaTime);
            CustomMoveV((curSpeed * curDir).Y * Engine.DeltaTime);

            yield return null;
        }

        state = StateTypes.Idle;
    }

    private IEnumerator MoveToTargetPosCoroutine()
    {
        float dist = (TargetPos - Position).Length();
        curDir = (TargetPos - Position).SafeNormalize();
        // 也就是没必要加到全速
        if (accelerationMoveDist * 2 >= dist)
        {
            // 1/2 at^2 == dist / 2
            float t = (float)Math.Sqrt(dist / a);
            float curMaxSpeed = a * t;
            float elapse = 0;
            while (elapse != t)
            {
                elapse = Calc.Approach(elapse, t, Engine.DeltaTime);
                curSpeed = (elapse / t) * curMaxSpeed;
                CustomMoveH((curSpeed * curDir).X * Engine.DeltaTime);
                CustomMoveV((curSpeed * curDir).Y * Engine.DeltaTime);

                yield return null;
            }

            elapse = 0;
            while (elapse != t)
            {
                elapse = Calc.Approach(elapse, t, Engine.DeltaTime);
                curSpeed = (1 - elapse / t) * curMaxSpeed;
                CustomMoveH((curSpeed * curDir).X * Engine.DeltaTime);
                CustomMoveV((curSpeed * curDir).Y * Engine.DeltaTime);

                yield return null;
            }
        }
        else // 距离够长, 足够加速减速
        {
            float elapse = 0;
            while (elapse != accelerationDuration)
            {
                elapse = Calc.Approach(elapse, accelerationDuration, Engine.DeltaTime);
                curSpeed = (elapse / accelerationDuration) * maxSpeed;
                CustomMoveH((curSpeed * curDir).X * Engine.DeltaTime);
                CustomMoveV((curSpeed * curDir).Y * Engine.DeltaTime);

                yield return null;
            }

            float moveDuration = (dist - accelerationMoveDist * 2) / maxSpeed;
            elapse = 0;
            while (elapse != moveDuration)
            {
                elapse = Calc.Approach(elapse, moveDuration, Engine.DeltaTime);
                curSpeed = maxSpeed;
                CustomMoveH((curSpeed * curDir).X * Engine.DeltaTime);
                CustomMoveV((curSpeed * curDir).Y * Engine.DeltaTime);

                yield return null;
            }

            elapse = 0;
            while (elapse != accelerationDuration)
            {
                elapse = Calc.Approach(elapse, accelerationDuration, Engine.DeltaTime);
                curSpeed = (1 - elapse / accelerationDuration) * maxSpeed;
                CustomMoveH((curSpeed * curDir).X * Engine.DeltaTime);
                CustomMoveV((curSpeed * curDir).Y * Engine.DeltaTime);

                yield return null;
            }
        }

        state = StateTypes.Idle;
        // CustomMoveTo(targetPos);
        // Position = targetPos;
    }


    public override void Render()
    {
        base.Render();
        // Draw.Rect(start, 10, 10, Color.Red);
        // Draw.Rect(end, 10, 10, Color.Red);
        // Draw.Rect(Position, 10, 10, Color.Red);
        // Draw.Rect(Position + offsetY, 10, 10, Color.Red);
        // Draw.Rect(LeftCliffside.Position, 10, 10, Color.Green);
    }

    private void CustomMoveH(float value, bool moveCeiling = true)
    {
        MoveH(value);
        // Logger.Log(LogLevel.Warn, "Test", "123");
        if (moveCeiling)
            ceiling?.MoveH(value);
    }

    private void CustomMoveV(float value, bool moveCeiling = true)
    {
        MoveV(value);
        if (moveCeiling)
            ceiling?.MoveV(value);
    }
}