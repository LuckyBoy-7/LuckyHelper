namespace LuckyHelper.Extensions;

public static class EntityDataExtensions
{
    /// <summary>
    /// 适配一些实体的老的Key
    /// </summary>
    /// <param name="data"></param>
    /// <param name="callback">key, 返回值, 默认值</param>
    /// <param name="keys">第一个为最新 key, 后续为老的 key</param>
    /// <typeparam name="T">要适配的数据类型</typeparam>
    /// <returns></returns>
    public static T Fit<T>(EntityData data, Func<string, T, T> callback, T defaultValue, params string[] keys)
    {
        for (int i = 0; i < keys.Length; i++)
        {
            if (data.Has(keys[i]))
            {
                return callback(keys[i], defaultValue);
            }
        }

        return defaultValue;
    }

    public static float FitFloat(this EntityData data, float defaultValue, params string[] keys) => Fit(data, data.Float, defaultValue, keys);
    public static Color FitColor(this EntityData data, Color defaultValue, params string[] keys) => Fit(data, data.HexColor, defaultValue, keys);
    public static bool FitBool(this EntityData data, bool defaultValue, params string[] keys) => Fit(data, data.Bool, defaultValue, keys);

    public static EntityData Clone(this EntityData orig)
    {
        var newData = new EntityData
        {
            Name = orig.Name,
            ID = orig.ID,
            Level = orig.Level, // 应用偏移
            Position = orig.Position, // 应用偏移
            Width = orig.Width,
            Height = orig.Height,
            Origin = orig.Origin,
            Nodes = orig.Nodes?.Select(node => node).ToArray()
        };
        if (orig.Values != null)
            newData.Values = new Dictionary<string, object>(orig.Values);

        return newData;
    }

    public static Vector2 Center(this EntityData orig)
    {
        return orig.Position + new Vector2(orig.Width, orig.Height) / 2;
    }


    public static Color HexColorWithAlpha(this EntityData entityData, string key, Color defaultValue = default)
    {
        if (!entityData.Has(key))
            return defaultValue;

        string hex = entityData.Attr(key);

        // 有猪啊, 加了 alpha 忘记处理原来的颜色了(
        if (hex.Length == 6)
            return Calc.HexToColor(hex);

        if (hex.All(char.IsLetterOrDigit) && hex.Length == 8)
        {
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            byte a = Convert.ToByte(hex.Substring(6, 2), 16);
            return new Color(r, g, b, 255) * (a / 255f);
        }

        return Color.White;
    }
}