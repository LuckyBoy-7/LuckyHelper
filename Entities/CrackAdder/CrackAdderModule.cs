using System.Text.RegularExpressions;
using LuckyHelper.Module;
using LuckyHelper.Utils;

namespace LuckyHelper.Entities.CrackAdder;

public static class CrackAdderModule
{
    private static readonly Dictionary<LevelData, char[,]> levelToSolidGrid = new();
    private static readonly Dictionary<int, List<Image>> crackAdderCachedImages = new();

    [Load]
    public static void Load()
    {
        Everest.Events.AssetReload.OnReloadLevel += OnReloadLevel;
    }


    [Unload]
    public static void Unload()
    {
        Everest.Events.AssetReload.OnReloadLevel -= OnReloadLevel;
    }

    private static void OnReloadLevel(Level level)
    {
        levelToSolidGrid.Clear();
        crackAdderCachedImages.Clear();
    }

    public static bool TryGetCrackImageCacheFrom(int id, out List<Image> images)
    {
        return crackAdderCachedImages.TryGetValue(id, out images);
    }

    public static void AddCrackImageCacheFrom(int id, List<Image> images)
    {
        crackAdderCachedImages[id] = images;
    }

    public static char[,] GetSolidGridWithLevelData(LevelData levelData)
    {
        if (!levelToSolidGrid.ContainsKey(levelData))
            GenerateSolidGridForLevel(levelData);
        return levelToSolidGrid[levelData];
    }

    private static void GenerateSolidGridForLevel(LevelData levelData)
    {
        int width = levelData.Bounds.Width / 8;
        int height = levelData.Bounds.Height / 8;
        char[,] solidGrid = new char[height, width];

        for (int i = 0; i < height; i++)
        for (int j = 0; j < width; j++)
            solidGrid[i, j] = '0';

        Regex regex = new Regex("\\r\\n|\\n\\r|\\n|\\r");
        string[] solidStringData = regex.Split(levelData.Solids);

        for (var i = 0; i < solidStringData.Length; i++)
        {
            for (var j = 0; j < solidStringData[i].Length; j++)
            {
                if (i >= 0 && i < height && j >= 0 && j < width)
                {
                    solidGrid[i, j] = solidStringData[i][j];
                }
            }
        }


        levelToSolidGrid[levelData] = solidGrid;
    }
}