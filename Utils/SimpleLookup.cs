using ExtendedVariants.Variants;

namespace LuckyHelper.Utils;

public class SimpleLookup
{
    private HashSet<string> lookup = new();
    private List<string> prefixes = new();
    private bool matchAll;

    public SimpleLookup()
    {
    }

    public SimpleLookup(List<string> items)
    {
        foreach (var item in items)
        {
            Add(item);
            if (matchAll)
                break;
        }
    }

    public void Add(string item)
    {
        if (item == "*")
        {
            matchAll = true;
        }

        if (item.EndsWith("*"))
        {
            prefixes.Add(item.Substring(0, item.Length - 1));
        }
        else
        {
            lookup.Add(item);
        }
    }

    public bool Contains(string item)
    {
        if (matchAll)
            return true;
        if (lookup.Contains(item))
            return true;
        foreach (var prefix in prefixes)
        {
            if (item.StartsWith(prefix))
                return true;
        }

        return false;
    }
}