using Celeste.Mod.Entities;
using Celeste.Mod.Registry.DecalRegistryHandlers;
using LuckyHelper.Modules;

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
        data.HexColor("color")
    )
    {
        // Logger.Warn("test", data.Attr("texture"));
        decalRegistryPaths = data.Attr("decalRegistryPaths");
    }


    public override void Added(Scene scene)
    {
        base.Added(scene);
        foreach (var path in decalRegistryPaths.Split(","))
        {
            ApplyDecalRegistry(path.Trim().ToLower());
        }
    }

    private void ApplyDecalRegistry(string path)
    {
        DecalRegistry.DecalInfo decalInfo;
        if (!CombinedDecalRegistryModule.RegisteredDecals.TryGetValue(path, out decalInfo))
        {
            return;
        }

        foreach (DecalRegistryHandler decalRegistryHandler in decalInfo.Handlers)
        {
            try
            {
                Logger.Warn("Test", decalRegistryHandler.Name);
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