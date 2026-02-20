using Celeste.Mod.Entities;
using Lucky.Kits.Extensions;
using LuckyHelper.Extensions;
using LuckyHelper.Utils;

namespace LuckyHelper.Entities.CrackAdder;

[CustomEntity("LuckyHelper/CrackAdder")]
public class CrackAdder : Entity
{
    private List<MTexture> crackLeftTextures;

    private List<MTexture> crackMiddleTextures;

    private List<MTexture> crackTestTextures;

    private float sideCrackDensity;
    private float middleCrackDensity;

    private string hideFlag;
    private Color color;

    private const int CrackGridSize = 2;
    private List<char> targetAppliedTileIDs;

    private List<Image> crackImages;
    private int id;

    private const char SpecialEmptyToken = '鳌';

    enum CrackType
    {
        Middle,
        Left,
        Top,
        Right,
        Bottom
    }


    public CrackAdder(EntityData entityData, Vector2 offset, EntityID entityId) : base(entityData.Position + offset)
    {
        string crackLeftTexturePath = entityData.Attr("crackLeftTexturePath", "decals/LuckyHelper/crack/left");
        crackLeftTextures = GFX.Game.GetAtlasSubtextures(crackLeftTexturePath);

        string crackMiddleTexturePath = entityData.Attr("crackMiddleTexturePath", "decals/LuckyHelper/crack/middle");
        crackMiddleTextures = GFX.Game.GetAtlasSubtextures(crackMiddleTexturePath);


        string crackTestTexturePath = entityData.Attr("crackTestTexturePath", "decals/LuckyHelper/crack/test");
        crackTestTextures = GFX.Game.GetAtlasSubtextures(crackTestTexturePath);


        sideCrackDensity = entityData.Float("sideCrackDensity");
        sideCrackDensity = Math.Clamp(sideCrackDensity, 0, 1);
        middleCrackDensity = entityData.Float("middleCrackDensity");
        middleCrackDensity = Math.Clamp(middleCrackDensity, 0, 1);

        targetAppliedTileIDs = entityData.ParseToStringList("targetAppliedTileIDs").Select(s => s[0]).ToList();

        // sideCrackDensity = 0.5f;
        // middleCrackDensity = 0f;

        hideFlag = entityData.Attr("hideFlag");
        color = entityData.HexColorWithAlpha("color");

        id = entityId.ID;
        Depth = entityData.Int("depth", Depth);
        Position = Vector2.Zero;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        GenerateCracks();
        UpdateVisible();
    }

    public override void Update()
    {
        base.Update();
        TryUpdateVisible();
    }

    private void TryUpdateVisible()
    {
        bool show = !this.Session().GetFlag(hideFlag);

        if (Visible != show)
        {
            UpdateVisible();
        }
    }

    private void UpdateVisible()
    {
        bool show = !this.Session().GetFlag(hideFlag);

        Visible = show;
        foreach (var crackImage in crackImages)
        {
            crackImage.Visible = show;
        }
    }

    private void GenerateCracks()
    {
        if (CrackAdderModule.TryGetCrackImageCacheFrom(id, out crackImages))
        {
            foreach (var crackImage in crackImages)
            {
                Add(crackImage);
            }

            return;
        }

        crackImages = new();
        LevelData levelData = this.Session().LevelData;

        char[,] solidGrid = CrackAdderModule.GetSolidGridWithLevelData(levelData);
        // int m = solidGrid.Length; 长度返回的居然是 x * y 总长度
        int m = solidGrid.GetLength(0);
        int n = solidGrid.GetLength(1);

        for (int i = 0; i < m; i++)
        {
            for (int j = 0; j < n; j++)
            {
                TryGenerateCrackImageAt(solidGrid, i, j);
            }
        }

        CrackAdderModule.AddCrackImageCacheFrom(id, crackImages);
    }

    private void TryGenerateCrackImageAt(char[,] solidGrid, int i, int j)
    {
        if (!SuitForCrack(solidGrid, i, j))
            return;


        LevelData levelData = this.Session().LevelData;
        Vector2 levelPosition = levelData.Position;

        if (!TryGetCrackImageAtWithDensity(solidGrid, i, j, out Image crack))
            return;

        Vector2 offset = Vector2.One * CrackGridSize / 2 * 8;
        crack.Position = levelPosition + new Vector2(j * 8, i * 8) + offset;
        // LogUtils.LogDebug(crack.Position.ToString());
        crackImages.Add(crack);
        Add(crack);

        for (int di = 0; di < CrackGridSize; di++)
        {
            for (int dj = 0; dj < CrackGridSize; dj++)
            {
                solidGrid[i + di, j + dj] = SpecialEmptyToken;
            }
        }
    }

    private bool CheckTileInTarget(char ch)
    {
        if (targetAppliedTileIDs.Count == 0)
            return true;
        return targetAppliedTileIDs.Contains(ch);
    }

    private bool TryGetCrackImageAtWithDensity(char[,] solidGrid, int i, int j, out Image crackImage)
    {
        crackImage = null;

        if (!Calc.Random.Chance(sideCrackDensity))
            return false;
        List<CrackType> filteredCrackTypes = GetPossibleCrackTypes(solidGrid, i, j);
        if (filteredCrackTypes.Contains(CrackType.Middle) && !Calc.Random.Chance(middleCrackDensity))
            return false;
        if (!filteredCrackTypes.Contains(CrackType.Middle) && !Calc.Random.Chance(sideCrackDensity))
            return false;

        CrackType targetCrackType = filteredCrackTypes.Choice();

        int rotationCount = GetRotationCountByCrackType(targetCrackType);
        MTexture texture = GetTextureByCrackType(targetCrackType);
        bool shouldFlipVertical = GetShouldFlipVerticalByCrackType(targetCrackType);

        crackImage = new Image(texture);
        crackImage.CenterOrigin();
        crackImage.FlipY = shouldFlipVertical;
        crackImage.Rotation = Calc.DegToRad * 90 * rotationCount;
        crackImage.Color = color;
        // crackImage.Color = Color.White * (Calc.Random.Next(255) /255f);

        return true;
    }

    private bool GetShouldFlipVerticalByCrackType(CrackType targetCrackType)
    {
        if (targetCrackType == CrackType.Middle)
            return false;
        return Calc.Random.Chance(0.5f);
    }

    private MTexture GetTextureByCrackType(CrackType targetCrackType)
    {
        // return crackTestTextures[0];
        if (targetCrackType == CrackType.Middle)
            return crackMiddleTextures.Choice();
        return crackLeftTextures.Choice();
    }

    private int GetRotationCountByCrackType(CrackType targetCrackType)
    {
        if (targetCrackType == CrackType.Middle)
            return Calc.Random.Next(4);
        return targetCrackType switch
        {
            CrackType.Left => 0,
            CrackType.Top => 1,
            CrackType.Right => 2,
            CrackType.Bottom => 3,
            _ => 0,
        };
    }

    private bool IsEmpty(char ch) => ch == '0';


    private List<CrackType> GetPossibleCrackTypes(char[,] solidGrid, int i, int j)
    {
        bool InGrid(int i, int j) => i >= 0 && i < solidGrid.GetLength(0) && j >= 0 && j < solidGrid.GetLength(1);

        bool IsEmptyColumn(int i, int j, int length)
        {
            for (int di = 0; di < length; di++)
            {
                if (InGrid(i, j) && !IsEmpty(solidGrid[i + di, j]))
                    return false;
            }

            return true;
        }

        bool IsEmptyRow(int i, int j, int length)
        {
            for (int dj = 0; dj < length; dj++)
            {
                if (InGrid(i, j) && !IsEmpty(solidGrid[i, j + dj]))
                    return false;
            }

            return true;
        }

        List<CrackType> filteredCrackTypes = new List<CrackType>();

        if (IsEmptyColumn(i, j - 1, CrackGridSize))
            filteredCrackTypes.Add(CrackType.Left);
        if (IsEmptyRow(i - 1, j, CrackGridSize))
            filteredCrackTypes.Add(CrackType.Top);
        if (IsEmptyColumn(i, j + CrackGridSize, CrackGridSize))
            filteredCrackTypes.Add(CrackType.Right);
        if (IsEmptyRow(i + CrackGridSize, j, CrackGridSize))
            filteredCrackTypes.Add(CrackType.Bottom);

        if (filteredCrackTypes.Count == 0)
            // middle 是只要空间够就可以随便放的, 但是如果有靠边的裂缝可能还是把位置让给别人更好
            filteredCrackTypes.Add(CrackType.Middle);

        return filteredCrackTypes;
    }

    private bool SuitForCrack(char[,] solidGrid, int i, int j)
    {
        char start = solidGrid[i, j];
        if (IsEmpty(start))
            return false;
        if (!CheckTileInTarget(solidGrid[i, j]))
            return false;

        for (int di = 0; di < CrackGridSize; di++)
        {
            for (int dj = 0; dj < CrackGridSize; dj++)
            {
                int ni = i + di;
                int nj = j + dj;
                bool outOfBound = ni < 0 || ni >= solidGrid.GetLength(0) || nj < 0 || nj >= solidGrid.GetLength(1);
                if (outOfBound)
                    return false;
                bool different = solidGrid[ni, nj] != start;
                if (different)
                    return false;
            }
        }

        return true;
    }

    private void AddCracksWithSolidGrid(string sessionLevel)
    {
    }

    private void TryBuildSolidGrid(LevelData levelData)
    {
    }
}