using Celeste.Mod.Entities;
using LuckyHelper.Components.EeveeLike;
using LuckyHelper.Extensions;
using LuckyHelper.Utils;
using YamlDotNet.Serialization.NodeDeserializers;


namespace LuckyHelper.Entities.EeveeLike;

public class RK4Circle
{
    // 参数
    public double R = 5.0; // 半径
    public double vT = 2.0; // 原有切向速度 (单位: 距离/秒)
    public double vAx = 1.0, vAy = 0.5; // 外加恒定速度向量
    public double theta0 = Math.PI / 6; // 初始角度 (弧度)
    public int dir = 1; // 正常情况是逆时针, 在蔚蓝中表现为顺时针, 这里的旋转方向指的是普通情况也就是数学意义上的

    // ODE 右边：dθ/dt
    public double ThetaDot(double theta)
    {
        double proj = -vAx * Math.Sin(theta) + vAy * Math.Cos(theta);
        return (vT * dir + proj) / R;
    }

    // 用 RK4 走一步
    public double RK4Step(double theta, double dt)
    {
        double k1 = ThetaDot(theta);
        double k2 = ThetaDot(theta + 0.5 * dt * k1);
        double k3 = ThetaDot(theta + 0.5 * dt * k2);
        double k4 = ThetaDot(theta + dt * k3);

        double delta = (dt / 6.0) * (k1 + 2 * k2 + 2 * k3 + k4);

        return theta + delta;
    }
}

[Tracked]
[CustomEntity("LuckyHelper/ToggleOrbitContainer")]
public class ToggleOrbitContainer : Actor, IContainer
{
    public EntityContainer Container => _Container;
    public EntityContainerMover _Container;

    public Vector2 Pivot;
    public float Radius;
    public float StartAngle;
    public float EndAngle;
    public float MinRadians;
    public float MaxRadians;

    public string MoveToEndFlag;
    public ConnectionTypes ConnectionType;
    public bool ToggleMoveToEndDir;

    /// <summary>
    /// 数学意义上, 符合之举的旋转方向
    /// </summary>
    public enum ConnectionTypes
    {
        Clockwise,
        AntiClockwise
    }

    public ControlTypes ControlType;

    public enum ControlTypes
    {
        ByFlag,
        AutoClockwise,
        AutoAntiClockwise,
        Pingpong,
    }

    // 我们规定 index 从 0 -> n 为顺时针方向
    public List<Vector2> DebugPositions = new();
    public const int CircleSegments = 36;
    public float Speed;
    public float WindForceXMultiplier;
    public float WindForceYMultiplier;
    public bool DrawNodes;
    public bool Debug;
    public int PingpongDir = 1;
    public RK4Circle circle;

    #region LineRenderer

    private LineRenderer _lineRenderer;

    public class LineRenderer
    {
        public ToggleOrbitContainer Parent;
        public string LineNodeSpritePath;

        private int LineNodeNumber;

        // public int LineGapNumber => _lineNodeNumber - 1; // 植树问题
        private List<MTexture> textures;
        private List<int> textureIndices = new();


        public LineRenderer(string path, int number)
        {
            LineNodeSpritePath = path;
            LineNodeNumber = Math.Max(2, number); // 至少两个节点
            textures = GFX.SpriteBank.Atlas.GetAtlasSubtextures(LineNodeSpritePath);
            for (int i = 0; i < LineNodeNumber; i++)
            {
                textureIndices.Add(Calc.Random.Range(0, textures.Count));
            }
        }

        public void Render()
        {
            if (textures.Count == 0)
                return;
            for (int i = 0; i < LineNodeNumber; i++)
            {
                MTexture texture = textures[textureIndices[i]];
                Vector2 pos = Parent.GetOrbitPositionByPercent((float)i / (LineNodeNumber - 1));
                texture.DrawCentered(pos);
            }
        }
    }

    #endregion

    public ToggleOrbitContainer(EntityData data, Vector2 offset) : base(data.Position + offset + new Vector2(data.Width / 2f, data.Height / 2f))
    {
        Collider = new Hitbox(data.Width, data.Height);
        Collider.Position = new Vector2(-Width / 2f, -Height / 2f);
        AllowPushing = false;

        Depth = data.Int("depth");

        Add(_Container = new EntityContainerMover(data)
        {
            DefaultIgnored = e => e is ToggleOrbitContainer,
            OnFit = OnFit
        });

        // y 轴朝上的 angle
        StartAngle = data.Float("startAngle").Mod(360);
        EndAngle = data.Float("endAngle").Mod(360);
        ConnectionType = data.Enum<ConnectionTypes>("connectionType");
        AdjustStartEndRadians();
        ControlType = data.Enum<ControlTypes>("controlType");

        MoveToEndFlag = data.Attr("moveToEndFlag");
        Speed = data.Float("speed");
        WindForceXMultiplier = data.Float("windForceXMultiplier");
        WindForceYMultiplier = data.Float("windForceYMultiplier");
        DrawNodes = data.Bool("drawNodes");

        if (DrawNodes)
            _lineRenderer = new LineRenderer(data.Attr("lineNodeSpritePath"), data.Int("lineNodeNumber")) { Parent = this };


        Pivot = data.NodesOffset(offset)[0] + this.HalfSize();
        Radius = Vector2.Distance(Pivot, Position);
        Debug = data.Bool("debug");
        if (Debug)
            for (int i = 0; i < CircleSegments; i++)
            {
                float percent = (float)i / CircleSegments;
                Vector2 position = GetOrbitPositionByPercent(percent);
                DebugPositions.Add(position);
            }

        circle = new RK4Circle
        {
            R = Radius,
            vT = Speed
        };
    }

    private void AdjustStartEndRadians()
    {
        float start = 0, end = 0;
        if (ConnectionType == ConnectionTypes.Clockwise)
        {
            if (StartAngle <= EndAngle)
            {
                start = EndAngle;
                end = StartAngle + 360;
            }
            else
            {
                start = EndAngle;
                end = StartAngle;
            }

            ToggleMoveToEndDir = true;
        }
        else if (ConnectionType == ConnectionTypes.AntiClockwise)
        {
            if (StartAngle < EndAngle)
            {
                start = StartAngle;
                end = EndAngle;
            }
            else
            {
                start = StartAngle;
                end = EndAngle + 360;
            }
        }

        StartAngle = start;
        EndAngle = end;

        MinRadians = Math.Min(StartAngle, EndAngle) * Calc.DegToRad;
        MaxRadians = Math.Max(StartAngle, EndAngle) * Calc.DegToRad;
    }


    public Vector2 GetOrbitPositionByPercent(float percent)
    {
        // 转化到蔚蓝里绘制对应的角度
        float start = -EndAngle, end = -StartAngle;

        float angle = Calc.LerpClamp(start, end, percent) * Calc.DegToRad;
        return Pivot + Calc.AngleToVector(angle, Radius);
    }

    public override void Render()
    {
        base.Render();
        if (Debug)
        {
            Draw.Circle(Pivot, 5, Color.Green, 3);
            Draw.Line(Pivot, DebugPositions[0], Color.Green);
            Draw.Line(Pivot, DebugPositions[^1], Color.Green);
            DrawUtils.DottedLine(DebugPositions[0], DebugPositions[^1], Color.Blue, 11);
            for (int i = 0; i < DebugPositions.Count - 1; i++)
            {
                Vector2 cur = DebugPositions[i];
                Vector2 next = DebugPositions[(i + 1) % DebugPositions.Count];
                Draw.Line(cur, next, Color.Red);
            }
        }

        _lineRenderer?.Render();
    }


    public override void Update()
    {
        base.Update();
        Move();
    }

    private int RadiansChangeDir
    {
        get
        {
            int dir = 0;
            if (ControlType == ControlTypes.ByFlag)
            {
                dir = this.Session().GetFlag(MoveToEndFlag) ? 1 : -1;
                // 正常情况就是逆时针在走, 所以如果选择顺时针则反转方向
                if (ToggleMoveToEndDir)
                {
                    dir *= -1;
                }
            }
            else if (ControlType == ControlTypes.AutoClockwise)
                dir = -1;
            else if (ControlType == ControlTypes.AutoAntiClockwise)
                dir = 1;
            else if (ControlType == ControlTypes.Pingpong)
            {
                dir = PingpongDir;
            }

            return dir;
        }
    }

    private void Move()
    {
        Vector2 wind = this.Level().Wind * new Vector2(WindForceXMultiplier, WindForceYMultiplier);
        circle.vAx = wind.X;
        circle.vAy = -wind.Y;
        circle.dir = RadiansChangeDir;
        circle.theta0 = circle.RK4Step(circle.theta0, Engine.DeltaTime);
        if (ControlType is ControlTypes.ByFlag or ControlTypes.Pingpong)
        {
            circle.theta0 = Calc.Clamp((float)circle.theta0, MinRadians, MaxRadians);
            if (ControlType == ControlTypes.Pingpong && (circle.theta0 == MinRadians || circle.theta0 == MaxRadians))
                PingpongDir *= -1;
        }
        else
        {
            circle.theta0 = MathUtils.WrapMod(circle.theta0, MinRadians, MaxRadians);
        }

        Vector2 relativePosition = Calc.AngleToVector((float)circle.theta0, Radius);
        relativePosition.Y *= -1;
        Vector2 newPosition = Pivot + relativePosition;

        if (newPosition != Position)
            _Container.DoMoveAction(() => Position = newPosition);
    }


    private void OnFit(Vector2 pos, float width, float height)
    {
        Position = new Vector2(pos.X + width / 2f, pos.Y + height / 2f);
        Collider.Position = new Vector2(-width / 2f, -height / 2f);
        Collider.Width = width;
        Collider.Height = height;
    }
}