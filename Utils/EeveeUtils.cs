namespace LuckyHelper;

public static class EeveeUtils
{
	public static Vector2 GetPosition(Entity entity)
	{
		return entity is Platform platform ? platform.ExactPosition : entity.Position;
	}

	public static Vector2 GetTrackBoost(Vector2 move, bool disableBoost)
	{
		return move * new Vector2(disableBoost ? 0f : 1f, 1f) + (move.X != 0f && move.Y == 0f && disableBoost ? Vector2.UnitY * 0.01f : Vector2.Zero);
	}

	public static Tuple<string, bool> ParseFlagAttr(string flag)
	{
		return flag.StartsWith("!") ? Tuple.Create(flag.Substring(1), true) : Tuple.Create(flag, false);
	}

	public static void ParseFlagAttr(string attr, out string flag, out bool notFlag)
	{
		var parsed = ParseFlagAttr(attr);
		flag = parsed.Item1;
		notFlag = parsed.Item2;
	}

	public static EntityData CloneEntityData(EntityData data, LevelData levelData = null)
	{
		var level = levelData ?? data.Level;

		return new EntityData
		{
			Name = data.Name,
			Level = levelData ?? data.Level,
			ID = data.ID,
			Position = data.Position + data.Level.Position - level.Position,
			Width = data.Width,
			Height = data.Height,
			Origin = data.Origin,
			Nodes = data.NodesOffset(data.Level.Position - level.Position),
			Values = data.Values == null ? new Dictionary<string, object>() : new Dictionary<string, object>(data.Values)
		};
	}

	public static T GetValueOfType<T>(Dictionary<Type, T> dict, Type type)
	{
		var success = dict.TryGetValue(type, out var value);

		if (success)
		{
			return value;
		}

		if (type.BaseType == null)
		{
			return default;
		}

		return GetValueOfType(dict, type.BaseType);
	}

	public static int? OptionalInt(EntityData data, string key, int? defaultValue = null)
	{
		if (!data.Has(key))
		{
			return defaultValue;
		}

		if (int.TryParse(data.Attr(key), out var result))
		{
			return result;
		}

		return null;
	}

	public static float? OptionalFloat(EntityData data, string key, float? defaultValue = null)
	{
		if (!data.Has(key))
		{
			return defaultValue;
		}

		if (float.TryParse(data.Attr(key), out var result))
		{
			return result;
		}

		return null;
	}

	public static Vector2? OptionalVector(EntityData data, string keyX, string keyY, Vector2? defaultValue = null)
	{
		if (!data.Has(keyX) || !data.Has(keyY))
		{
			return defaultValue;
		}

		var definedX = float.TryParse(data.Attr(keyX), out var x);
		var definedY = float.TryParse(data.Attr(keyY), out var y);

		if (definedX || definedY)
		{
			return new Vector2(x, y);
		}

		return null;
	}
}