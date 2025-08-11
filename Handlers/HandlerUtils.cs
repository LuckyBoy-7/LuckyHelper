using LuckyHelper.Handlers;

namespace LuckyHelper;

public static class HandlerUtils
{
	public static bool DoAs<T>(IEntityHandler handler, Action<T> action, Action<Entity> or = null)
	{
		if (handler is T castedHandler)
		{
			action(castedHandler);
			return true;
		}
		else if (or != null && handler.Entity != null && handler.Container != null &&
			handler.Container.IsFirstHandler(handler) && !handler.Container.HasHandlerFor<T>(handler.Entity))
		{
			or(handler.Entity);
		}
		return false;
	}

	public static R GetAs<T, R>(IEntityHandler handler, Func<T, R> func, Func<Entity, R> or = null)
	{
		if (handler is T castedHandler)
		{
			return func(castedHandler);
		}
		else if (or != null && handler.Entity != null && handler.Container != null &&
			handler.Container.IsFirstHandler(handler) && !handler.Container.HasHandlerFor<T>(handler.Entity))
		{
			return or(handler.Entity);
		}
		return default;
	}
}
