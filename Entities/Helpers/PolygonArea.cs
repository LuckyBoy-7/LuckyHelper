using Microsoft.Xna.Framework.Graphics;

namespace LuckyHelper.Entities;

public class PolygonArea : Entity
{
    private VirtualRenderTarget buffer;
    private List<VertexPositionColor> fillPoints = new();
    private List<Vector2> outlinePoints = new();
    private Vector2 min = Vector2.One * int.MaxValue;
    private Vector2 max = Vector2.One * int.MinValue;

    // todo: 可修改部分
    public Color FillColor = Color.Cyan;
    public Color OutlineColor = Color.White;
    public float Alpha = 0.3f;
    public float OutlineWith = 3;
    // public float Scale = 1.1f;

    public PolygonArea(List<Vector2> poses, Color fillColor, Color outlineColor, float alpha, float outlineWith)
    {
        FillColor = fillColor;
        OutlineColor = outlineColor;
        Alpha = alpha;
        OutlineWith = outlineWith;
        
        Depth = 202202202;
        // get points
        var tmp = poses.Select(pos => new VertexPositionColor(new Vector3(pos.X, pos.Y, 0), FillColor * Alpha)).ToList();
        for (var i = 1; i < tmp.Count - 1; i++)
        {
            fillPoints.Add(tmp[0]);
            fillPoints.Add(tmp[i]);
            fillPoints.Add(tmp[i + 1]);
        }

        outlinePoints = poses;
        // for (var i = 1; i < tmp.Count - 1; i++)
        // {
        // outlinePoints.Add(new(tmp[i].Position.X, tmp[i].Position.Y));
        // outlinePoints.Add(new(tmp[i + 1].Position.X, tmp[i + 1].Position.Y));
        // }

        foreach (var vec in poses)
        {
            min = Vector2.Min(min, vec);
            max = Vector2.Max(max, vec);
        }
    }

    public override void Render()
    {
        base.Render();
        Matrix cam = ((Level)Scene).Camera.Matrix;
        // 铸币了, 用缩放的话描边不是等长的, 所以还是直接画线好点 
        // Vector3 gravityPoint = GetGravitPoint(fillPoints.Select(p => p.Position).ToList());
        // Matrix matrix = Matrix.CreateTranslation(-gravityPoint) * Matrix.CreateScale(Scale) * Matrix.CreateTranslation(gravityPoint) * cam;
        // GFX.DrawVertices(matrix, outlinePoints.ToArray(), outlinePoints.Count);


        int n = outlinePoints.Count;
        for (var i = 0; i < n; i++)
        {
            Draw.Line(outlinePoints[i], outlinePoints[(i + 1) % n], OutlineColor * Alpha, OutlineWith);
        }

        GFX.DrawVertices(cam, fillPoints.ToArray(), fillPoints.Count);
    }

    public bool IsBoxInAABB(float left, float right, float top, float bottom) => !(right < min.X || left > max.X || top > max.Y && bottom < min.Y);

    public bool IsPointInAABB(Vector2 pos) => pos.X <= max.X && pos.X >= min.X && pos.Y >= min.Y && pos.Y <= max.Y;

    public bool PointInPolygon(Vector2 pos) => PointInPolygon(pos, outlinePoints);

    private bool PointInPolygon(Vector2 pos, List<Vector2> points)
    {
        if (!IsPointInAABB(pos))
            return false;

        // pos 向右发射射线
        int n = points.Count;
        int c = 0;
        for (int i = 0; i < n; i++)
        {
            Vector2 a = points[i];
            Vector2 b = points[(i + 1) % n];
            // 出发点在顶点上
            if (pos == a || pos == b)
                return true;
            // 两端点在当前点的两侧  
            if ((a.Y < pos.Y && pos.Y <= b.Y) || (b.Y < pos.Y && pos.Y <= a.Y))
            {
                // 射线对应的直线和线段交点的x坐标
                float x = a.X + (pos.Y - a.Y) / (b.Y - a.Y) * (b.X - a.X);
                // 点在线段上
                if (x == pos.X)
                    return true;

                if (x > pos.X)
                    c += 1;
            }
        }

        return c % 2 != 0;
    }



    // private Vector3 GetGravitPoint(List<Vector3> lstPol)
    // {
    //     double n, i;
    //     double x1, y1, x2, y2, x3, y3;
    //     double sum_x = 0, sum_y = 0, sum_s = 0;
    //     x1 = lstPol[0].X;
    //     y1 = lstPol[0].Y;
    //     x2 = lstPol[1].X;
    //     y2 = lstPol[1].Y;
    //
    //     n = lstPol.Count;
    //     int k = 2;
    //     for (i = 1; i <= n - 2; i++)
    //     {
    //         x3 = lstPol[k].X;
    //         y3 = lstPol[k].Y;
    //
    //         // 三角形面积
    //         double s = ((x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1)) / 2f;
    //         sum_x += (x1 + x2 + x3) * s;
    //         sum_y += (y1 + y2 + y3) * s;
    //         sum_s += s;
    //         x2 = x3;
    //         y2 = y3;
    //         k++;
    //     }
    //
    //     return new Vector3((float)(sum_x / sum_s / 3f), (float)(sum_y / sum_s / 3f), 0);
    // }
}