using Celeste.Mod.Entities;
using LuckyHelper.Components.EeveeLike;
using LuckyHelper.Extensions;


namespace LuckyHelper.Entities.EeveeLike;

[Tracked]
[CustomEntity("LuckyHelper/ToggleOrbitContainer")]
public class ToggleOrbitContainer : Actor, IContainer
{
    public EntityContainer Container => _Container;
    public EntityContainerMover _Container;

    public Vector2 Pivot;
    public float Distance;
    public float StartAngle;
    public float EndAngle;
    public string MoveToEndFlag;
    public ConnectionTypes ConnectionType;

    public enum ConnectionTypes
    {
        Clockwise,
        AntiClockwise
    }

    // 我们规定 index 从 0 -> n 为顺时针方向
    public List<Vector2> Positions = new();
    public int CurrentIndex;
    public int CircleSegments;
    public float Speed;
    public float WindForceXMultiplier;
    public float WindForceYMultiplier;
    public bool DrawNodes;
    public bool Debug;

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
        // Depth = 1000;

        Add(_Container = new EntityContainerMover(data)
        {
            DefaultIgnored = e => e is ToggleOrbitContainer,
            OnFit = OnFit
        });

        StartAngle = data.Float("startAngle");
        EndAngle = data.Float("endAngle");
        MoveToEndFlag = data.Attr("moveToEndFlag");
        ConnectionType = data.Enum<ConnectionTypes>("connectionType");
        CircleSegments = Math.Max(3, data.Int("circleSegments"));
        Speed = data.Float("speed");
        WindForceXMultiplier = data.Float("windForceXMultiplier");
        WindForceYMultiplier = data.Float("windForceYMultiplier");
        DrawNodes = data.Bool("drawNodes");
        Debug = data.Bool("debug");

        if (DrawNodes)
            _lineRenderer = new LineRenderer(data.Attr("lineNodeSpritePath"), data.Int("lineNodeNumber")) { Parent = this };
        if (StartAngle > EndAngle)
            (StartAngle, EndAngle) = (EndAngle, StartAngle);

        Pivot = data.NodesOffset(offset)[0];
        Distance = Vector2.Distance(Pivot, Position);
        for (int i = 0; i < CircleSegments; i++)
        {
            float percent = (float)i / CircleSegments;
            Vector2 position = GetOrbitPositionByPercent(percent);
            Positions.Add(position);

            // 将 container 吸附到最近的点上
            if (Vector2.Distance(position, Position) < Vector2.Distance(Positions[CurrentIndex], Position))
            {
                CurrentIndex = i;
            }
        }

        CurrentIndex = Calc.Clamp(CurrentIndex, 1, CircleSegments - 2);
    }

    public Vector2 GetOrbitPositionByPercent(float percent)
    {
        float start, end;

        if (ConnectionType == ConnectionTypes.Clockwise)
        {
            start = StartAngle;
            end = EndAngle;
        }
        else if (ConnectionType == ConnectionTypes.AntiClockwise)
        {
            start = EndAngle;
            end = StartAngle + 360;
        }
        else
        {
            return Vector2.Zero;
        }

        float angle = Calc.LerpClamp(start, end, percent) * Calc.DegToRad;
        return Pivot + Calc.AngleToVector(angle, Distance);
    }

    public override void Render()
    {
        base.Render();
        if (Debug)
        {
            
            Draw.Circle(Pivot, 5, Color.Green, 3);
            for (int i = 0; i < Positions.Count; i++)
            {
                Vector2 cur = Positions[i];
                Vector2 next = Positions[(i + 1) % Positions.Count];
                Draw.Line(cur, next, Color.Red);
            }
        }

        _lineRenderer?.Render();
    }

    private void OnFit(Vector2 pos, float width, float height)
    {
        Position = new Vector2(pos.X + width / 2f, pos.Y + height / 2f);
        Collider.Position = new Vector2(-width / 2f, -height / 2f);
        Collider.Width = width;
        Collider.Height = height;
    }

    public override void Update()
    {
        base.Update();
        int dir = this.Session().GetFlag(MoveToEndFlag) ? 1 : -1;
        Vector2 wind = this.Level().Wind * new Vector2(WindForceXMultiplier, WindForceYMultiplier);
        float sum = 0;
        Vector2 speedDir = Positions[CurrentIndex + dir] - Positions[CurrentIndex];
        speedDir.Normalize();
        Vector2 velocity = Speed * speedDir;
        Vector2 newVelocity = velocity + speedDir * (Vector2.Dot(velocity, wind) / Speed);
        if (float.Sign(velocity.X) != float.Sign(newVelocity.X)) // 说明风太大了, 改变向了
            dir *= -1;


        float speed = newVelocity.Length();

        // 0 和 n - 1 我们留空方便写代码
        while (CurrentIndex + dir > 0 && CurrentIndex + dir < Positions.Count - 1)
        {
            int nextIndex = CurrentIndex + dir;
            sum += Vector2.Distance(Positions[CurrentIndex], Positions[nextIndex]);
            CurrentIndex = nextIndex;
            if (sum > speed * Engine.DeltaTime)
                break;
        }


        _Container.DoMoveAction(() => Position = Positions[CurrentIndex]);
    }
}