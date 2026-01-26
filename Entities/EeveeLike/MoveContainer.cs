using System.Diagnostics.Tracing;
using Celeste.Mod.Entities;
using LuckyHelper.Components.EeveeLike;
using LuckyHelper.Extensions;
using LuckyHelper.Modules;
using LuckyHelper.Utils;
using YamlDotNet.Serialization.NodeDeserializers;


namespace LuckyHelper.Entities.EeveeLike;

public class MoveInListHelperConditionPart
{
    public enum ConditionTypes
    {
        ByTriggeredFlag, // 获得 flag 后移动到下一个点, 并消耗
        ByExistingFlag, // 存在 flag 即可
        Always, // 自动向前移动
    }

    private ConditionTypes conditionType;
    private string flag;
    private int leftTriggeredCount;
    private Session session;

    public MoveInListHelperConditionPart(ConditionTypes conditionType, string flag, Session session)
    {
        this.conditionType = conditionType;
        this.session = session;

        this.flag = flag;
    }

    public bool IsTriggered()
    {
        if (conditionType == ConditionTypes.ByTriggeredFlag)
        {
            bool res = session.GetFlag(flag);
            session.SetFlag(flag, false);
            return res;
        }

        if (conditionType == ConditionTypes.ByExistingFlag)
        {
            return session.GetFlag(flag);
        }

        return true;
    }
}

public class MoveInListHelperMovePart
{
    public class PathCalculator
    {
        protected Vector2 From { get; set; }
        protected Vector2 To { get; set; }
        public int NextIndex { get; protected set; }
        protected float Elapse { get; set; }
        protected List<Vector2> Positions { get; set; }

        public virtual void InitWithIndices(int currentIndex, int idealNextIndex, int moveDir)
        {
        }


        public virtual Vector2 GetPositionAt(float t) => Vector2.Lerp(From, To, t);

        public virtual float GetPathLength() => Vector2.Distance(From, To);

        /// <returns>是否运动到了端点</returns>
        public bool Move(float dt, float duration, Ease.Easer ease, Action<Vector2> setter)
        {
            Elapse += dt;
            float t = ease(Elapse / duration);

            if (t >= 1f)
            {
                Elapse = 0;
                setter(To);
                return true;
            }

            setter(GetPositionAt(t));
            return false;
        }

        public void InitWithPositions(List<Vector2> dataPositions)
        {
            Positions = dataPositions;
        }
    }

    // 直线路径
    public class StraightPathCalculator : PathCalculator
    {
        public override void InitWithIndices(int currentIndex, int idealNextIndex, int moveDir)
        {
            NextIndex = idealNextIndex;
            From = Positions[currentIndex];
            To = Positions[NextIndex];
        }
    }

    // 直线路径
    public class AlongPathCalculator : PathCalculator
    {
        public override void InitWithIndices(int currentIndex, int idealNextIndex, int moveDir)
        {
            NextIndex = int.Sign(idealNextIndex - currentIndex) + currentIndex;
            From = Positions[currentIndex];
            To = Positions[NextIndex];
        }
    }

    // 贝塞尔路径
    public class BezierPathCalculator : PathCalculator
    {
        private SimpleCurve curve = new SimpleCurve();

        private Dictionary<SimpleCurve, int> bezierToLengthCache = new Dictionary<SimpleCurve, int>();

        public override void InitWithIndices(int currentIndex, int idealNextIndex, int moveDir)
        {
            NextIndex = (moveDir * 2 + currentIndex).Mod(Positions.Count);
            int controlIndex = (moveDir * 1 + currentIndex).Mod(Positions.Count);

            From = Positions[currentIndex];
            To = Positions[NextIndex];

            curve.Begin = From;
            curve.End = To;
            curve.Control = Positions[controlIndex];
        }

        public override Vector2 GetPositionAt(float t)
        {
            return curve.GetPoint(t);
        }

        public override float GetPathLength()
        {
            if (bezierToLengthCache.TryGetValue(curve, out var pathLength))
                return pathLength;

            float length = curve.GetLengthParametric(30);
            bezierToLengthCache[curve] = (int)length;
            return length;
        }
    }


    public class MovePartData
    {
        public enum MoveTypes
        {
            BySpeed,
            ByDuration
        }

        public MoveTypes MoveType;

        public enum MoveAlongTypes
        {
            AlongPath,
            StraightLine,
            Bezier
        }


        public MoveAlongTypes MoveAlongType;
        public List<Vector2> Positions;

        public float Speed;

        public float Duration;
        public Ease.Easer EaseFunc;


        public Func<int, int> NextIndexFunc;
        public Func<int> MoveDirFunc;
    }

    private MovePartData data;

    // private float elapsed = 0f;
    private int currentIndex = 0;
    public Vector2 CurrentPosition { get; private set; }
    public bool Moving { get; private set; } // 表示当前位置正在坐标中移动还是静止在某个坐标上

    private PathCalculator pathCalculator;

    public MoveInListHelperMovePart(MovePartData data)
    {
        this.data = data;

        CurrentPosition = data.Positions[0];
        pathCalculator = data.MoveAlongType switch
        {
            MovePartData.MoveAlongTypes.AlongPath => new AlongPathCalculator(),
            MovePartData.MoveAlongTypes.StraightLine => new StraightPathCalculator(),
            MovePartData.MoveAlongTypes.Bezier => new BezierPathCalculator(),
            _ => null
        };
        pathCalculator.InitWithPositions(data.Positions);
    }


    public void Update()
    {
        // 说明当前已经禁止移动了
        int idealNextIndex = data.NextIndexFunc(currentIndex);
        if (idealNextIndex == -1 || idealNextIndex == currentIndex)
            return;

        if (!Moving)
        {
            int moveDir = data.MoveDirFunc();
            pathCalculator.InitWithIndices(currentIndex, idealNextIndex, moveDir);
            Moving = true;
        }

        float duration = data.Duration;
        if (data.MoveType == MovePartData.MoveTypes.BySpeed)
            duration = pathCalculator.GetPathLength() / data.Speed;
        if (pathCalculator.Move(Engine.DeltaTime, duration, data.EaseFunc, pos => CurrentPosition = pos))
        {
            Moving = false;
            currentIndex = pathCalculator.NextIndex;
        }
        //
        // Vector2 from = data.Positions[currentIndex];
        // Vector2 to = data.Positions[actualNextIndex];
        // if (data.MoveType == MovePartData.MoveTypes.ByDuration)
        // {
        //     elapsed += Engine.DeltaTime;
        //     float t = Math.Min(elapsed / data.Duration, 1f);
        //     t = data.EaseFunc(t);
        //
        //     CurrentPosition = Vector2.Lerp(from, to, t);
        //     if (t >= 1f)
        //     {
        //         elapsed = 0f;
        //         currentIndex = actualNextIndex;
        //         if (actualNextIndex == idealNextIndex) // 这才算真的到了
        //     }
        // }
        // else if (data.MoveType == MovePartData.MoveTypes.BySpeed)
        // {
        //     float distanceToMove = data.Speed * Engine.DeltaTime;
        //     CurrentPosition = Calc.Approach(CurrentPosition, to, distanceToMove);
        //     if (CurrentPosition == to)
        //     {
        //         currentIndex = actualNextIndex;
        //         if (actualNextIndex == idealNextIndex) // 这才算真的到了
        //             Moving = false;
        //     }
        // }
    }
}

public class MoveInListHelperDirectionPart
{
    public enum DirectionTypes
    {
        StopAtEnd,
        Loop,
        PingPong,
        ToCertainFlag
    }

    private DirectionTypes directionType;
    private int dir = 1;
    private int positionCount;
    private Session session;
    private List<string> flags;

    public MoveInListHelperDirectionPart(DirectionTypes directionType, int positionCount, string flags, Session session)
    {
        this.directionType = directionType;
        this.positionCount = positionCount;
        this.flags = ParseUtils.ParseCommaSeperatedStringToList(flags);
        this.session = session;
    }

    public int GetMoveDirIndex() => dir;

    public int GetNextIndex(int currentIndex)
    {
        int n = positionCount;
        if (directionType == DirectionTypes.Loop)
        {
            return (currentIndex + 1) % n;
        }

        if (directionType == DirectionTypes.PingPong)
        {
            if (currentIndex == n - 1)
                dir = -1;
            else if (currentIndex == 0)
                dir = 1;

            return currentIndex + dir;
        }

        if (directionType == DirectionTypes.StopAtEnd)
        {
            if (currentIndex == n - 1)
                return -1;
            return currentIndex + 1;
        }

        if (directionType == DirectionTypes.ToCertainFlag)
        {
            for (var i = 0; i < flags.Count; i++)
            {
                var flag = flags[i];
                if (session.GetFlag(flag))
                {
                    return i;
                }
            }
        }

        // 未知情况
        return -1;
    }
}

public class MoveInListHelper
{
    public MoveInListHelperConditionPart Control;
    public MoveInListHelperMovePart Move;
    public MoveInListHelperDirectionPart Direction;

    public void Update()
    {
        if (Move.Moving)
            Move.Update();
        else if (Control.IsTriggered())
        {
            Move.Update();
        }
    }
}

[Tracked]
[CustomEntity("LuckyHelper/MoveContainer")]
public class MoveContainer : Actor, IContainer
{
    public EntityContainer Container => _Container;
    public EntityContainerMover _Container;

    private MoveInListHelper moveHelper;
    private List<Vector2> positions;

    private EntityData data;
    private Vector2 offset;
    private bool generatContainerAlongPath;


    public MoveContainer(EntityData data, Vector2 offset) : base(data.Position + offset + new Vector2(data.Width / 2f, data.Height / 2f))
    {
        this.data = data;
        this.offset = offset;
        generatContainerAlongPath = data.Bool("generateContainerAlongPath");
        Collider = new Hitbox(data.Width, data.Height);
        Collider.Position = new Vector2(-Width / 2f, -Height / 2f);
        AllowPushing = false;

        Depth = -1000000;

        Add(_Container = new EntityContainerMover(data)
        {
            DefaultIgnored = e => e is MoveContainer,
            OnFit = OnFit
        });

        var halfSize = this.HalfSize();
        positions = data.NodesWithPosition(offset + halfSize).ToList();
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        moveHelper = new MoveInListHelper();
        moveHelper.Control = new MoveInListHelperConditionPart(
            data.Enum<MoveInListHelperConditionPart.ConditionTypes>("conditionType"),
            data.Attr("conditionFlag"),
            this.Session());
        moveHelper.Direction = new MoveInListHelperDirectionPart(
            data.Enum<MoveInListHelperDirectionPart.DirectionTypes>("directionType"),
            positions.Count,
            data.Attr("directionFlags"),
            this.Session());
        moveHelper.Move = new MoveInListHelperMovePart(new MoveInListHelperMovePart.MovePartData()
        {
            MoveType = data.Enum<MoveInListHelperMovePart.MovePartData.MoveTypes>("moveType"),
            Positions = positions,
            Speed = data.Float("speed"),
            Duration = data.Float("duration"),
            EaseFunc = EaseModule.EaseTypes[data.Attr("ease", "Linear")],
            NextIndexFunc = moveHelper.Direction.GetNextIndex,
            MoveDirFunc = moveHelper.Direction.GetMoveDirIndex,
            MoveAlongType = data.Enum<MoveInListHelperMovePart.MovePartData.MoveAlongTypes>("moveAlongType")
        });

        if (generatContainerAlongPath)
        {
            for (int i = 1; i < positions.Count; i++)
            {
                MoveContainer moveContainer = new MoveContainer(data, offset);
                Scene.Add(moveContainer);

                List<Vector2> newPositions = new List<Vector2>();
                int n = positions.Count;
                for (int j = 0; j < n; j++)
                {
                    newPositions.Add(positions[(i + j) % n]);
                }

                moveContainer.Position = newPositions[0];
                moveContainer.positions = newPositions;
                moveContainer.generatContainerAlongPath = false;
            }
        }
    }


    public override void Update()
    {
        base.Update();
        moveHelper.Update();
        Vector2 targetPosition = moveHelper.Move.CurrentPosition;
        if (targetPosition != Position)
            _Container.DoMoveAction(() => { NaiveMove(targetPosition - ExactPosition); });
    }


    private void OnFit(Vector2 pos, float width, float height)
    {
        Position = new Vector2(pos.X + width / 2f, pos.Y + height / 2f);
        Collider.Position = new Vector2(-width / 2f, -height / 2f);
        Collider.Width = width;
        Collider.Height = height;
    }
}