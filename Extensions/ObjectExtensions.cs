using LuckyHelper.Utils;

namespace LuckyHelper.Extensions;

public static class ObjectExtensions
{
    public static string BriefTypeName(this object obj)
    {
        return ParseUtils.TypeToBriefName(obj.GetType().ToString());
    }
}