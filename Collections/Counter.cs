namespace Lucky.Collections;

public class Counter<T> : DefaultDict<T, int>
{

    public Counter() : base(() => 0)
    {
    }
    
    public Counter(IEnumerator<T> it) : base(() => 0)
    {
        while (it.MoveNext())
        {
            this[it.Current] += 1;
        }
    }

    public static Counter<T> operator |(Counter<T> c1, Counter<T> c2)
    {
        Counter<T> retval = new();
        foreach (var key in GetCombinedKeys(c1, c2))
            retval[key] = Math.Max(c1[key], c2[key]);
        return retval;
    }

    public static Counter<T> operator &(Counter<T> c1, Counter<T> c2)
    {
        Counter<T> retval = new();
        foreach (var key in GetCommonKeys(c1, c2))
            retval[key] = Math.Min(c1[key], c2[key]);
        return retval;
    }

    public static Counter<T> operator -(Counter<T> c1, Counter<T> c2)
    {
        Counter<T> retval = new();
        foreach (var key in GetCombinedKeys(c1, c2))
            retval[key] = c1[key] - c2[key];
        return retval;
    }

    public static Counter<T> operator +(Counter<T> c1, Counter<T> c2)
    {
        Counter<T> retval = new();
        foreach (var key in GetCombinedKeys(c1, c2))
            retval[key] = c1[key] + c2[key];
        return retval;
    }

    private static HashSet<T> GetCombinedKeys(Counter<T> c1, Counter<T> c2)
    {
        HashSet<T> retval = new();
        foreach (var counterKey in c1.Keys)
            retval.Add(counterKey);
        foreach (var counterKey in c2.Keys)
            retval.Add(counterKey);
        return retval;
    }

    private static HashSet<T> GetCommonKeys(Counter<T> c1, Counter<T> c2)
    {
        HashSet<T> allKeys = GetCombinedKeys(c1, c2);
        HashSet<T> retval = new();
        foreach (var key in allKeys)
        {
            if (c1.ContainsKey(key) && c2.ContainsKey(key))
                retval.Add(key);
        }

        return retval;
    }
}