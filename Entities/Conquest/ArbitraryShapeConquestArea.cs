using Celeste.Mod.Entities;
using Lucky.Kits.Extensions;

namespace LuckyHelper.Entities;

[CustomEntity("LuckyHelper/ArbitraryShapeConquestArea")]
[Tracked]
public class ArbitraryShapeConquestArea : Entity
{
    private PolygonArea polygonArea;
    private List<Vector2> points;

    public float CurAmount = 0;
    public HashSet<string> BlueCampHashset;
    public HashSet<string> RedCampHashset;
    public HashSet<string> IgnoreCollidableWhiteListHashset;

    private ConquestProgressBar UI;

    public string ID => "Conquest" + ConquestAreaName;

    public bool Solved
    {
        get => SceneAs<Level>().Session.GetFlag(ID + "_Solved");
        set
        {
            if (value)
                SceneAs<Level>().Session.SetFlag(ID + "_Solved");
        }
    }
    // todo: 以下都是可供mapper调整的部分

    // self
    public float TotalAmount = 100;
    public float ConquestSpeed = 10;
    public string BlueCamp = "Player";
    public string RedCamp = "Seeker, BadelineOldsite, Strawberry";
    public string FlagOnCompleted = "ConquestTestFlag";
    public bool IgnoreCollidable = true;
    public string IgnoreCollidableWhiteList = "BadelineOldsite";

    public bool UseSpriteShapeAsFallback = true;

    // polygon
    public Color PolygonFillColor;
    public Color PolygonOutlineColor;
    public float PolygonAlpha = 0.3f;

    public float PolygonOutlineWith = 3;

    // UI
    public string ConquestAreaName = "A1";
    public string ConquestAreaId = "A";

    public float IdFontSize = 0.7f;
    public Color IdFontColor = Color.White;
    public float PlaceFontSize = 0.4f;
    public Color PlaceFontColor = Color.White;

    public float UITotalWidth = 1000; // filling的宽高
    public float UITotalHeight = 40;
    public float UIOutlineWidth = 8;

    public Color UIFillColor = Color.Cyan * 0.5f;
    public Color UIBackgroundColor = Color.Gray;
    public Color UIOutlineColor = Color.Black;

    public ArbitraryShapeConquestArea(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        // data
        TotalAmount = data.Float("totalAmount");
        ConquestSpeed = data.Float("conquestSpeed");
        BlueCamp = data.Attr("blueCamp");
        RedCamp = data.Attr("redCamp");
        FlagOnCompleted = data.Attr("flagOnCompleted");
        IgnoreCollidable = data.Bool("ignoreCollidable");
        IgnoreCollidableWhiteList = data.Attr("ignoreCollidableWhiteList");
        UseSpriteShapeAsFallback = data.Bool("useSpriteShapeAsFallback");
        // polygon
        PolygonFillColor = data.HexColor("polygonFillColor");
        PolygonOutlineColor = data.HexColor("polygonOutlineColor");
        PolygonAlpha = data.Float("polygonAlpha");
        PolygonOutlineWith = data.Float("polygonOutlineWith");
        // UI
        ConquestAreaName = data.Attr("conquestAreaName");
        ConquestAreaId = data.Attr("conquestAreaId");

        IdFontSize = data.Float("idFontSize");
        IdFontColor = data.HexColor("idFontColor");
        PlaceFontSize = data.Float("placeFontSize");
        PlaceFontColor = data.HexColor("placeFontColor");

        UITotalWidth = data.Float("UITotalWidth"); // filling的宽高
        UITotalHeight = data.Float("UITotalHeight");
        UIOutlineWidth = data.Float("UIOutlineWidth");

        UIFillColor = data.HexColor("UIFillColor");
        UIBackgroundColor = data.HexColor("UIBackgroundColor");
        UIOutlineColor = data.HexColor("UIOutlineColor");

        Depth = -1000000;
        points = new List<Vector2>() { data.Position + offset };
        points.Extend(data.Nodes.Select(node => node + offset).ToList());

        BlueCampHashset = ParseClassName(BlueCamp);
        RedCampHashset = ParseClassName(RedCamp);
        IgnoreCollidableWhiteListHashset = ParseClassName(IgnoreCollidableWhiteList);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        Scene.Add(polygonArea = new PolygonArea(points, PolygonFillColor, PolygonOutlineColor, PolygonAlpha, PolygonOutlineWith));

        // // polygon
        // polygonArea.FillColor = PolygonFillColor;
        // polygonArea.OutlineColor = PolygonOutlineColor;
        // polygonArea.Alpha = PolygonAlpha;
        // polygonArea.OutlineWith = PolygonOutlineWith;

        if (Scene.Tracker.GetEntity<ConquestProgressBar>() == null)
            Scene.Add(new ConquestProgressBar());
        CurAmount = SceneAs<Level>().Session.GetCounter(ID);

        // foreach (var entity in Scene.Entities)
        // {
        //     Logger.Log(LogLevel.Warn, "Test", entity.GetType().Name);
        // }
    }

    public override void Removed(Scene scene)
    {
        SceneAs<Level>().Session.SetCounter(ID, (int)CurAmount);
        base.Removed(scene);
    }

    private bool flag = true;

    public override void Update()
    {
        base.Update();
        if (!Solved)
        {
            int blue = 0, red = 0;
            foreach (var entity in Scene.Entities)
            {
                // if (RedCampHashset.Contains(entity.GetType().Name))
                //     Logger.Log(LogLevel.Warn, "Test", entity.GetType().Name);
                if (BlueCampHashset.Contains(entity.GetType().Name) && CollideWith(entity))
                {
                    blue += 1;
                }
                else if (RedCampHashset.Contains(entity.GetType().Name) && CollideWith(entity))
                {
                    red += 1;
                }
            }

            // Logger.Log(LogLevel.Warn, "Test", "blue" + blue);
            // Logger.Log(LogLevel.Warn, "Test", "red" + red);
            float speed = ConquestSpeed * (blue - red);
            CurAmount = Calc.Clamp(CurAmount + speed * Engine.DeltaTime, 0, TotalAmount);

            if (CurAmount == TotalAmount)
            {
                Solved = true;
                SceneAs<Level>().Session.SetFlag(FlagOnCompleted);
            }
        }


        // UI
        if (UI == null)
            UI = Scene.Tracker.GetEntity<ConquestProgressBar>();
        UI.realK = CurAmount / TotalAmount;
        UI.ConquestAreaName = ConquestAreaName;
        UI.ConquestAreaId = ConquestAreaId;
        UI.IdFontSize = IdFontSize;
        UI.IdFontColor = IdFontColor;
        UI.PlaceFontSize = PlaceFontSize;
        UI.PlaceFontColor = PlaceFontColor;
        UI.TotalWidth = UITotalWidth;
        UI.TotalHeight = UITotalHeight;
        UI.OutlineWidth = UIOutlineWidth;
        UI.FillColor = UIFillColor;
        UI.BackgroundColor = UIBackgroundColor;
        UI.OutlineColor = UIOutlineColor;
    }


    private HashSet<string> ParseClassName(string str)
    {
        // Logger.Log(LogLevel.Warn, "Test", str);
        string[] lst = str.Split(',');
        HashSet<string> res = new();
        foreach (string s in lst)
        {
            res.Add(s.Trim());
            // Logger.Log(LogLevel.Warn, "Test", s);
        }

        return res;
    }


    private bool CollideWith(Entity entity)
    {
        if (entity == null)
            return false;
        // 圆形那些也当作矩形来看, 应该不会有人发现吧(
        Collider collider = entity.Collider;
        float left;
        float right;
        float top;
        float bottom;

        if (collider == null)
        {
            if (UseSpriteShapeAsFallback)
            {
                left = entity.Left;
                right = entity.Right;
                top = entity.Top;
                bottom = entity.Bottom;
            }
            else
                return false;
        }
        else
        {
            if (entity.Collidable || (IgnoreCollidable && IgnoreCollidableWhiteListHashset.Contains(entity.GetType().Name)))
            {
                left = collider.AbsoluteLeft;
                right = collider.AbsoluteRight;
                top = collider.AbsoluteTop;
                bottom = collider.AbsoluteBottom;
            }
            else
                return false;
        }

        // 如果都不在aabb里则直接退出
        if (!polygonArea.IsBoxInAABB(left, right, top, bottom))
            return false;
        // 先判断四个角和中间的, 概率最大
        if (
            polygonArea.PointInPolygon(new Vector2(left, top))
            || polygonArea.PointInPolygon(new Vector2(right - 1, top))
            || polygonArea.PointInPolygon(new Vector2(left, bottom - 1))
            || polygonArea.PointInPolygon(new Vector2(right - 1, bottom - 1))
            || polygonArea.PointInPolygon(new Vector2((left + right) / 2, (top + bottom) / 2))
        )
            return true;

        return false;
        // for (float x = left; x < right; x++)
        // {
        //     for (float y = top; y < bottom; y++)
        //     {
        //         if (polygonArea.PointInPolygon(new Vector2(x, y)))
        //             return true;
        //     }
        // }
        //
        // return false;
    }

    public override void Render()
    {
        base.Render();
        // int size = 4;
        // foreach (var pos in points)
        // {
        // Draw.Rect(pos, size, size, Color.Green);
        // }

        // Draw.Rect(Position, size, size, Color.Red);
    }
}