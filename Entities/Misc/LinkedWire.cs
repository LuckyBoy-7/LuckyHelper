using Celeste.Mod.Entities;
using Celeste.Mod.Helpers;
using Lucky.Kits.Extensions;

namespace LuckyHelper.Entities.Misc;

[CustomEntity("LuckyHelper/LinkedWire")]
[Tracked]
public class LinkedWire : Entity
{
    private const int Resolution = 50;
    private const float LinkDistanceThreshold = 2f;
    private static readonly Vector2 ControlDefaultOffset = new(0f, 24f);

    public Color Color;
    public SimpleCurve Curve;

    private readonly float sineX;
    private readonly float sineY;


    private readonly List<Vector2> curvePoints = new();
    private readonly List<EndPoint> childrenEndPoints = new();

    private class EndPoint
    {
        public Vector2 position;
        public LinkedWire currentWire;

        public LinkedWire parentWire;
        public float positionPercentInParent;
    }

    private EndPoint begin;
    private EndPoint end;
    private bool hasHandled;
    private bool ignoreLink;
    private float wiggleSpeedMultiplier;
    private float wiggleStrengthMultiplier;
    private bool HasLinkedToParents => begin.parentWire != null || end.parentWire != null;

    private List<LinkedWire> renderOrderWires = new();

    public LinkedWire(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        Vector2 from = data.Position + offset;
        Vector2 to = data.Nodes[0] + offset;
        // bool above = data.Bool("above");
        Color = data.HexColor("color");
        ignoreLink = data.Bool("ignoreLink");
        Depth = data.Int("depth");
        wiggleSpeedMultiplier = data.Float("wiggleSpeedMultiplier");
        wiggleStrengthMultiplier = data.Float("wiggleStrengthMultiplier");

        begin = new EndPoint()
        {
            position = from,
            currentWire = this,
        };
        end = new EndPoint()
        {
            position = to,
            currentWire = this,
        };

        Curve = new SimpleCurve(from, to, (from + to) / 2 + ControlDefaultOffset);

        Random random = new((int)Math.Min(from.X, to.X));
        sineX = random.NextFloat(4f);
        sineY = random.NextFloat(4f);
    }

    public override void Awake(Scene scene)
    {
        base.Awake(scene);

        if (hasHandled || ignoreLink)
            return;

        // 因为有可能切板的时候读到之前的 wire, 所以还是得通过 handled 去拿 "all" Wires
        List<LinkedWire> allWires = Scene.Tracker.GetEntities<LinkedWire>().Cast<LinkedWire>().Where(wire => !wire.hasHandled).ToList();
        List<LinkedWire> needHandledWires = allWires.Where(wire => !wire.ignoreLink).ToList();

        // 链接, 保证无环
        foreach (var fromWire in needHandledWires)
        {
            fromWire.hasHandled = true;
            foreach (var toWire in needHandledWires)
            {
                if (fromWire == toWire)
                    continue;


                if (fromWire.begin.parentWire == null)
                {
                    toWire.TryLinkedByEndPoint(fromWire.begin);
                }

                if (fromWire.end.parentWire == null)
                {
                    toWire.TryLinkedByEndPoint(fromWire.end);
                }

                bool allHasLinked = fromWire.begin.parentWire != null && fromWire.end.parentWire != null;
                if (allHasLinked)
                    break;
            }
        }

        // 分组
        List<List<LinkedWire>> groups = new();
        HashSet<LinkedWire> visited = new();
        foreach (var wire in allWires)
        {
            if (visited.Contains(wire))
                continue;

            List<LinkedWire> group = new();
            Queue<LinkedWire> queue = new();
            queue.Enqueue(wire);

            while (queue.Count > 0)
            {
                LinkedWire current = queue.Dequeue();

                if (visited.Contains(current))
                    continue;
                visited.Add(current);
                group.Add(current);

                if (current.begin.parentWire != null)
                    queue.Enqueue(current.begin.parentWire);
                if (current.end.parentWire != null)
                    queue.Enqueue(current.end.parentWire);
                foreach (var childEndPoint in current.childrenEndPoints)
                {
                    queue.Enqueue(childEndPoint.currentWire);
                }
            }

            groups.Add(group);
        }

        foreach (List<LinkedWire> group in groups)
        {
            List<LinkedWire> noParentWires = group.Where(wire => !wire.HasLinkedToParents).ToList();
            List<LinkedWire> order = new();

            // 做一个拓扑排序

            while (noParentWires.Count > 0)
            {
                LinkedWire freedWire = noParentWires.Pop();

                order.Add(freedWire);

                foreach (var child in freedWire.childrenEndPoints)
                {
                    LinkedWire origWire = child.currentWire;
                    child.parentWire = null;
                    if (!origWire.HasLinkedToParents)
                        noParentWires.Add(origWire);
                }
            }

            order[0].renderOrderWires = order;
        }
    }

    public override void Render()
    {
        if (ignoreLink)
        {
            UpdateCurve();
            if (!IsVisible())
            {
                return;
            }

            RenderWire();
        }
        else
        {
            // update 逻辑还是得放 render 里, 这样切板也能正常跑, 还不用加 tag
            foreach (var wire in renderOrderWires)
            {
                wire.UpdateCurve();
                wire.UpdateChildrenEndPoints();

                if (!wire.IsVisible())
                {
                    continue;
                }

                wire.RenderWire();
            }
        }
    }


    private void InitializeCurvePoints()
    {
        for (int i = 0; i <= Resolution; i++)
        {
            float percent = i / (float)Resolution;
            Vector2 point = Curve.GetPoint(percent);
            curvePoints.Add(point);
        }
    }

    private void TryLinkedByEndPoint(EndPoint endPoint)
    {
        // 防止出现环形引用
        if (HasLinkedTo(endPoint.currentWire))
        {
            return;
        }

        if (curvePoints.Count == 0)
            InitializeCurvePoints();

        for (int i = 0; i < curvePoints.Count; i++)
        {
            if (Vector2.Distance(endPoint.position, curvePoints[i]) < LinkDistanceThreshold)
            {
                float percent = i / (float)Resolution;
                endPoint.parentWire = this;
                endPoint.positionPercentInParent = percent;
                childrenEndPoints.Add(endPoint);
                return;
            }
        }
    }

    /// <summary>
    /// 我们是是否直接/间接的链接过目标 wire 
    /// </summary>
    /// <returns></returns>
    private bool HasLinkedTo(LinkedWire toWire)
    {
        if (toWire == null)
            return false;
        if (begin.parentWire == toWire || end.parentWire == toWire)
            return true;
        if (begin.parentWire != null && begin.parentWire.HasLinkedTo(toWire))
            return true;
        return end.parentWire != null && end.parentWire.HasLinkedTo(toWire);
    }

    private void UpdateChildrenEndPoints()
    {
        foreach (var child in childrenEndPoints)
        {
            child.position = Curve.GetPoint(child.positionPercentInParent);
        }
    }

    private void UpdateCurve()
    {
        Level level = SceneAs<Level>();
        Vector2 windForce = CalculateWindForce(level);
        float windAdjustment = CalculateWindAdjustment(level);

        Vector2 controlOffset = windForce * windAdjustment;

        Curve.Begin = begin.position;
        Curve.End = end.position;
        Curve = Curve with { Control = (Curve.Begin + Curve.End) / 2f + ControlDefaultOffset + controlOffset };
    }

    private Vector2 CalculateWindForce(Level level)
    {
        float windX = (float)Math.Sin(sineX + level.WindSineTimer * 2f * wiggleSpeedMultiplier);
        float windY = (float)Math.Sin(sineY + level.WindSineTimer * 2.8f * wiggleSpeedMultiplier);
        return new Vector2(windX, windY) * 8f;
    }

    private float CalculateWindAdjustment(Level level)
    {
        return (level.Wind != Vector2.Zero ? level.VisualWind / 100f : 1f) * wiggleStrengthMultiplier;
    } 

    private void RenderWire()
    {
        Vector2 previousPoint = Curve.Begin;
        int n = 16;
        for (int i = 1; i <= n; i++)
        {
            float percent = i / (float)n;
            Vector2 currentPoint = Curve.GetPoint(percent);
            if (i == n)
            {
                // 好像是 draw.line 有时候就是会长一点
                currentPoint -= new Vector2(1, 1);
            }

            Draw.Line(previousPoint, currentPoint, Color);
            previousPoint = currentPoint;
        }
    }

    private bool IsVisible()
    {
        return CullHelper.IsCurveVisible(Curve, 2f);
    }
}