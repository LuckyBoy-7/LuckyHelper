namespace LuckyHelper.Extensions;

internal static class SceneExtensions
{
    public static Player GetPlayer(this Scene scene) => scene.Tracker.GetEntity<Player>();

    public static Level GetLevel(this Scene scene)
    {
        return scene switch
        {
            Level level => level,
            LevelLoader levelLoader => levelLoader.Level,
            _ => null
        };
    }

    public static Session GetSession(this Scene scene)
    {
        return scene switch
        {
            Level level => level.Session,
            LevelLoader levelLoader => levelLoader.session,
            LevelExit levelExit => levelExit.session,
            AreaComplete areaComplete => areaComplete.Session,
            _ => null
        };
    }
}
