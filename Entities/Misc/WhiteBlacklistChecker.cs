using LuckyHelper.Utils;

namespace LuckyHelper.Entities.Misc;

public class WhiteBlacklistChecker
{
    private List<string> whiteList;
    private List<string> blackList;

    public WhiteBlacklistChecker(EntityData entityData)
    {
        whiteList = entityData.ParseToStringList("whiteList");
        blackList = entityData.ParseToStringList("blackList");
    }

    public bool Check(object obj) => Check(obj.GetType().Name);

    public bool Check(string typeName)
    {
        if (blackList.Contains(typeName))
            return false;
        if (whiteList.Count == 0 || whiteList.Contains(typeName))
            return true;
        return false;
    }
}