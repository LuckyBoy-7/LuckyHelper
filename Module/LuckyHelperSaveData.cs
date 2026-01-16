namespace LuckyHelper.Module;

public class LuckyHelperSaveData : EverestModuleSaveData
{
    public HashSet<string> FilledJamJarSIDs { get; set; } = new();
}