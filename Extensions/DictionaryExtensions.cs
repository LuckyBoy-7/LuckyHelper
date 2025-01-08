namespace LuckyHelper.Extensions;

public static class DictionaryExtensions
{
    public static int GetInt<T>(this Dictionary<T, int> dict, T key)
    {
        if (dict.ContainsKey(key))
            return dict[key];
        return 0;
    }

    public static void AddInt<T>(this Dictionary<T, int> dict, T key, int number)
    {
        if (!dict.ContainsKey(key))
            dict[key] = 0;
        dict[key] += number;
    }
    
    public static float GetFloat<T>(this Dictionary<T, float> dict, T key)
    {
        if (dict.ContainsKey(key))
            return dict[key];
        return 0;
    }

    public static void AddFloat<T>(this Dictionary<T, float> dict, T key, float number)
    {
        if (!dict.ContainsKey(key))
            dict[key] = 0;
        dict[key] += number;
    }
}