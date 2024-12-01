using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework.Input;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/CustomGondola")]
public class CustomGondola : Gondola
{
    private Vector2 start;
    private Vector2 end;
    private Vector2 _realStart;

    private Vector2 RealStart
    {
        get
        {
            if (!autoFindStart)
                return Vector2.Lerp(start, end, minPositionPercent).Floor();

            return _realStart;
        }
    }

    private Vector2 RealEnd => Vector2.Lerp(start, end, maxPositionPercent).Floor();
    private readonly Vector2 offsetY = new Vector2(0, -52);

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
    private float curSpeed;
    private float a;
    private Vector2 dir;

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
    private bool autoFindStart = false;
    private bool canInteractOnMove = true;
    private bool addCeiling = true;

    private float minPositionPercent = 0.05f;
    private float maxPositionPercent = 0.8f;
    private float startPositionPercent = 0.2f;
    private float rotationSpeed = 0.7f;

    private float accelerationDuration = 2; // 起步和减速的时间
    private float moveDuration = 6; // 中间段的移动时间, 因为加速是匀加速的所所以位移曲线是个等腰梯形, 面积为(accelerationDuration + moveDuration) * curSpeed = dist


    public CustomGondola(EntityData data, Vector2 offset) : base(data, offset)
    {
        autoFindStart = data.Bool("autoFindStart");
        canInteractOnMove = data.Bool("canInteractOnMove");
        addCeiling = data.Bool("addCeiling");
        minPositionPercent = data.Float("minPositionPercent");
        maxPositionPercent = data.Float("maxPositionPercent");
        startPositionPercent = data.Float("startPositionPercent");
        rotationSpeed = data.Float("rotationSpeed");
        accelerationDuration = data.Float("accelerationDuration");
        moveDuration = data.Float("moveDuration");


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
        Add(interact = new TalkComponent(new Rectangle(-24, -8, 48, 8), new Vector2(-0.5f, -20f), this.Interact));
    }

    private void Interact(Player obj)
    {
        ChangeState();
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        if (addCeiling)
            Scene.Add(ceiling);

        LeftCliffside.Position = start;
        RightCliffside.Position = end;
        start += new Vector2(34, -6) - offsetY;
        end += new Vector2(-35, 0) - offsetY;

        // todo:到时候记得处理percent 距离
        float dist = (start - end).Length();
        maxSpeed = dist / (accelerationDuration + moveDuration);
        accelerationMoveDist = maxSpeed * accelerationDuration / 2;
        a = maxSpeed / accelerationDuration;

        // 自动获取初始位置
        if (autoFindStart)
        {
            Position = start;
            dir = (end - start).SafeNormalize();
            while (LeftCliffside.Y != Position.Y)
            {
                CustomMoveH(dir.X * Engine.DeltaTime, false);
                CustomMoveV(dir.Y * Engine.DeltaTime, false);
            }

            _realStart = Position;
        }
        else
        {
            Position = Vector2.Lerp(RealStart, RealEnd, Calc.Clamp(startPositionPercent, minPositionPercent, maxPositionPercent)).Floor();
        }

        if (addCeiling)
        {
            ceiling.Position = Position + new Vector2(0, -CeilingOffsetY);
            // DynamicData thisData = DynamicData.For(this);
            // DynamicData otherData = DynamicData.For(ceiling);
            // otherData.Set("movementCounter", thisData.Get<Vector2>("movementCounter"));
        }
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

        if (Vector2.Distance(TargetPos, Position) < 10)
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
            CustomMoveH((curSpeed * dir).X * Engine.DeltaTime);
            CustomMoveV((curSpeed * dir).Y * Engine.DeltaTime);

            yield return null;
        }
        state = StateTypes.Idle;
    }

    private IEnumerator MoveToTargetPosCoroutine()
    {
        float dist = (TargetPos - Position).Length();
        dir = (TargetPos - Position).SafeNormalize();
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
                CustomMoveH((curSpeed * dir).X * Engine.DeltaTime);
                CustomMoveV((curSpeed * dir).Y * Engine.DeltaTime);

                yield return null;
            }

            elapse = 0;
            while (elapse != t)
            {
                elapse = Calc.Approach(elapse, t, Engine.DeltaTime);
                curSpeed = (1 - elapse / t) * curMaxSpeed;
                CustomMoveH((curSpeed * dir).X * Engine.DeltaTime);
                CustomMoveV((curSpeed * dir).Y * Engine.DeltaTime);

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
                CustomMoveH((curSpeed * dir).X * Engine.DeltaTime);
                CustomMoveV((curSpeed * dir).Y * Engine.DeltaTime);

                yield return null;
            }

            float moveDuration = (dist - accelerationMoveDist * 2) / maxSpeed;
            elapse = 0;
            while (elapse != moveDuration)
            {
                elapse = Calc.Approach(elapse, moveDuration, Engine.DeltaTime);
                curSpeed = maxSpeed;
                CustomMoveH((curSpeed * dir).X * Engine.DeltaTime);
                CustomMoveV((curSpeed * dir).Y * Engine.DeltaTime);

                yield return null;
            }

            elapse = 0;
            while (elapse != accelerationDuration)
            {
                elapse = Calc.Approach(elapse, accelerationDuration, Engine.DeltaTime);
                curSpeed = (1 - elapse / accelerationDuration) * maxSpeed;
                CustomMoveH((curSpeed * dir).X * Engine.DeltaTime);
                CustomMoveV((curSpeed * dir).Y * Engine.DeltaTime);

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