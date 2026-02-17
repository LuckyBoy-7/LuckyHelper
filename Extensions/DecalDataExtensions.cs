namespace LuckyHelper.Extensions;

public static class DecalDataExtensions
{
    public static DecalData Clone(this DecalData orig)
    {
        var newData = new DecalData
        {
            Texture = orig.Texture,
            Position = orig.Position,
            Scale = orig.Scale, // 应用偏移
            Rotation = orig.Rotation, // 应用偏移
            ColorHex = orig.ColorHex,
            Depth = orig.Depth,
            Parallax = orig.Parallax,
        };

        return newData;
    }
}