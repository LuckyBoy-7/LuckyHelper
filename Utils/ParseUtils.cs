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
}