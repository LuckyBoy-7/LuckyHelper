namespace LuckyHelper.Utils;

public static class ParseUtils
{
    public static List<string> ParseCommaSeperatedStringToList(string str) => str.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    public static HashSet<string> ParseTypesStringToBriefNames(string str)
    {
        HashSet<string> types = new();
        foreach (var fullName in str.Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            types.Add(TypeToBriefName(fullName));
        }

        return types;
    }

    public static string TypeToBriefName(string fullName)
    {
        int i = fullName.LastIndexOf(".");
        if (i != -1)
            return fullName.Substring(i + 1);
        return fullName;
    }

    private static List<T> ParseToList<T>(EntityData data, string key, Func<string, T> converter)
    {
        string[] split = data.Attr(key).Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return split.Select(converter).ToList();
    }

    public static List<Color> ParseToColorList(this EntityData data, string key) => ParseToList(data, key, Calc.HexToColor);

    public static List<string> ParseToStringList(this EntityData data, string key) => ParseToList(data, key, s => s);
}