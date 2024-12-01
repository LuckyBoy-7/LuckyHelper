namespace LuckyHelper.Entities;

// 想了下还是把进度条跟area关联起来吧
[Tracked]
public class ConquestProgressBar : Entity
{
    public Color CurFillColor = Color.Cyan * 0.5f;
    public Color CurBackgroundColor = Color.Gray;
    public Color CurOutlineColor = Color.Black;


    public float curK = 0; // 进度
    public float realK = 0;

    public Vector2 PivotPos = new Vector2(0, -200);
    public Vector2 StartPivotPos = new Vector2(0, -200);
    public Vector2 EndPivotPos = Vector2.Zero;
    public float SpaceToTop = 80; // filling 相对于顶部的距离
    public float CurWidth = 0; // filling的宽高


    // public MTexture icon;
    // public Color IconColor = Color.Red;
    // public float IconSize = 2;
    
    // todo: 可修改部分
    
    public string ConquestAreaId = "A";
    public string ConquestAreaName = "along the river";
    
    public float IdFontSize = 0.7f;
    public Color IdFontColor = Color.White;
    public float PlaceFontSize = 0.4f;
    public Color PlaceFontColor = Color.White;

    // 这个宽是内部filling的宽, 外部的描边通过outlineWith计算
    public float TotalWidth = 1000; // filling的宽高
    public float TotalHeight = 40;
    public float OutlineWidth = 8;
    
    public Color FillColor = Color.Cyan * 0.5f;
    public Color BackgroundColor = Color.Gray;
    public Color OutlineColor = Color.Black;

    public ConquestProgressBar()
    {
        Tag = Tags.Global | Tags.HUD | Tags.TransitionUpdate;
        // icon = GFX.Gui["LuckyHelper/conquest_icon"];
    }

    public override void Render()
    {
        base.Render();
        Vector2 backgroundTopLeft = new Vector2(960 - CurWidth / 2, SpaceToTop) + PivotPos;
        Vector2 outlineTopLeft = backgroundTopLeft - Vector2.One * OutlineWidth;
        Vector2 midTop = new Vector2(960, backgroundTopLeft.Y );
        Vector2 midBottom = new Vector2(960, backgroundTopLeft.Y + TotalHeight);


        // outline
        Draw.Rect(outlineTopLeft, CurWidth + OutlineWidth * 2, TotalHeight + OutlineWidth * 2, CurOutlineColor);
        // background
        Draw.Rect(backgroundTopLeft, CurWidth, TotalHeight, CurBackgroundColor);
        // fill
        Draw.Rect(backgroundTopLeft, CurWidth * curK, TotalHeight, CurFillColor);

        // icon
        // icon.Draw(midBottom, new Vector2((float)icon.Width / 2, (float)icon.Height / 2), IconColor, IconSize);
        ActiveFont.Draw(
            ConquestAreaId, midTop + new Vector2(0, -35), Vector2.One / 2, Vector2.One * IdFontSize, IdFontColor
        );
        // ActiveFont.Draw(
        //     ConquestAreaId, midBottom + new Vector2(0, -4), Vector2.One / 2, Vector2.One * IconFontSize * ConquestAreaId.Length * 0.35f, Color.White
        // );
        ActiveFont.Draw(ConquestAreaName, midBottom + new Vector2(0, 30) , Vector2.One / 2, Vector2.One * PlaceFontSize, PlaceFontColor);
    }

    public override void Update()
    {
        base.Update();
        bool shouldShowUp = Scene.Tracker.GetEntity<ArbitraryShapeConquestArea>() != null;
        PivotPos = Vector2.Lerp(PivotPos, shouldShowUp ? EndPivotPos : StartPivotPos, 0.05f);

        curK = Calc.LerpClamp(curK, realK, 0.1f);
        CurWidth = Calc.LerpClamp(CurWidth, TotalWidth, 0.1f);
        CurBackgroundColor = Color.Lerp(CurBackgroundColor, BackgroundColor, 0.1f);
        CurFillColor = Color.Lerp(CurFillColor, FillColor, 0.1f);
        CurOutlineColor = Color.Lerp(CurOutlineColor, OutlineColor, 0.1f);
    }
}