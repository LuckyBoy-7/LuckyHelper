using Celeste.Mod.Entities;
using Celeste.Mod.Registry.DecalRegistryHandlers;
using LuckyHelper.Module;
using LuckyHelper.Modules;
using LuckyHelper.Utils;

namespace LuckyHelper.Entities.Decals;

[CustomEntity("LuckyHelper/DecalWithCombinedRegistry")]
[TrackedAs(typeof(Decal))]
[Tracked]
public class DecalWithCombinedRegistry : Decal
{
    private string decalRegistryPaths;

    public DecalWithCombinedRegistry(EntityData data, Vector2 offset) : base(
        data.Attr("texture"),
        data.Position + offset,
        new Vector2(data.Float("scaleX"), data.Float("scaleY")),
        data.Int("depth"),
        data.Float("rotation"),
        HexToColorWithAlpha(data.Attr("color"))
    )
    {
        // Logger.Warn("test", data.Attr("texture"));
        decalRegistryPaths = data.Attr("decalRegistryPaths");
    }

    public static Color HexToColorWithAlpha(string hex)
    {
        if (hex.All(char.IsLetterOrDigit))
        {
            if (hex.Length == 8)
            {
                byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                byte a = Convert.ToByte(hex.Substring(6, 2), 16);
                return new Color(r, g, b, 255) * (a / 255f);
            }
        
            // 有猪啊, 加了 alpha 忘记处理原来的颜色了(
            if (hex.Length == 6)
            {
                byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                return new Color(r, g, b, 255);
            }
        }
       

        return Color.White;
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        foreach (var path in decalRegistryPaths.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            ApplyDecalRegistry(path.ToLower());
        }
    }

    private void ApplyDecalRegistry(string path)
    {
        DecalRegistry.DecalInfo decalInfo;
 

        if (!CombinedDecalRegistryModule.RegisteredDecals.TryGetValue(path, out decalInfo))
        {
            LogUtils.LogWarning($"Can't find decal registry id '{path}' for the decal at position {Position}.");
            return;
        }

        foreach (DecalRegistryHandler decalRegistryHandler in decalInfo.Handlers)
        {
            try
            {
                decalRegistryHandler.ApplyTo(this);
            }
            catch (Exception ex)
            {
                LevelEnter.ErrorMessage = Dialog.Get("postcard_decalregerror", null).Replace("((property))", decalRegistryHandler.Name).Replace("((decal))", path);
                Logger.Warn("Decal Registry", "Failed to apply property '" + decalRegistryHandler.Name + "' to " + path);
                Logger.LogDetailed(ex, null);
            }
        }
    }
}