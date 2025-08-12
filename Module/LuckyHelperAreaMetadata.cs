namespace LuckyHelper.Module;

public class LuckyHelperYaml
{
    public LuckyHelperAreaMetadata LuckyHelperAreaMetaData { get; set; } = new LuckyHelperAreaMetadata();
}

public class LuckyHelperAreaMetadata
{
    public static LuckyHelperAreaMetadata TryGetMetadata(Session session)
    {
        if (!Everest.Content.TryGet($"Maps/{session.MapData.Filename}.meta", out ModAsset asset))
            return null;
        if (!(asset?.PathVirtual?.StartsWith("Maps") ?? false)) return null;
        if (!(asset?.TryDeserialize(out LuckyHelperYaml meta) ?? false)) return null;
        return meta?.LuckyHelperAreaMetaData;
    }

    public string DefaultTextboxPath { get; set; } = "textbox/default";
    public string DefaultMiniTextboxPath { get; set; } = "textbox/default_mini";
}