namespace LuckyHelper.Utils;

public static class MiscUtils
{
    public static bool TryGetLevelName(out string levelName)
    {
        levelName = null;
        if (TryGetSession(out Session session))
        {
            levelName = session.Level;
            return true;
        }

        return false;
    }

    public static bool TryGetSession(out Session session)
    {
        session = null;
        if (Engine.Scene is Level level)
        {
            session = level.Session;
            return true;
        }

        if (Engine.Scene is LevelLoader loader)
        {
            session = loader.session;
            return true;
        }

        return false;
    }
}