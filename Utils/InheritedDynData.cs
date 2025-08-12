using MonoMod.Utils;
using System.Collections;
using System.Reflection;

namespace LuckyHelper;

public sealed class InheritedDynData : IDisposable, IEnumerable<KeyValuePair<string, object>>, IEnumerable
{
	private static readonly Dictionary<Type, Type[]> _TypeMap = new();

	private readonly Type[] _Types;

	private DynamicData TopData;
	private DynamicData[] _Data;

	public InheritedDynData(object obj) : this(obj.GetType(), obj, true) { }
	public InheritedDynData(Type type, object obj, bool keepAlive)
	{
		var typeMap = _TypeMap;
		lock (typeMap)
		{
			if (!_TypeMap.TryGetValue(type, out _Types))
			{
				var types = new List<Type>();
				foreach (var other in Assembly.GetEntryAssembly().GetTypesSafe())
				{
					if (other.IsAssignableFrom(type))
					{
						types.Add(other);
					}
				}
				// 先父类, 后子类
				types.Sort((a, b) => !a.IsAssignableFrom(b) ? -1 : 0);
				_Types = types.ToArray();
				_TypeMap.Add(type, _Types);
			}
		}
		_Data = new DynamicData[_Types.Length];
		for (var i = 0; i < _Types.Length; i++)
		{
			var currentType = _Types[i];
			_Data[i] = new DynamicData(currentType, obj, true);
			if (currentType == type)
			{
				TopData = _Data[i];
			}
		}
	}

	public bool TryGet(string name, out object value)
	{
		foreach (var data in _Data)
		{
			if (data.TryGet(name, out value))
			{
				return true;
			}
		}
		value = null;
		return false;
	}

	public bool TryGet<T>(string name, out T value)
	{
		var success = TryGet(name, out var result);
		value = success ? (T)result : default;
		return success;
	}

	public object Get(string name)
	{
		TryGet(name, out var result);
		return result;
	}

	public T Get<T>(string name)
	{
		TryGet(name, out T result);
		return result;
	}

	public object Set(string name, object value)
	{
		foreach (var data in _Data)
		{
			if (data.TryGet(name, out _))
			{
				data.Set(name, value);
				return value;
			}
		}
		TopData.Set(name, value);
		return value;
	}

	public T Set<T>(string name, T value)
	{
		return (T)Set(name, (object)value);
	}

	public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
	{
		var seen = new HashSet<string>();
		foreach (var data in _Data)
		{
			foreach (var pair in data)
			{
				if (!seen.Contains(pair.Key))
				{
					yield return pair;
					seen.Add(pair.Key);
				}
			}
		}
		yield break;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	#region IDisposable Support
	private bool disposedValue = false; // To detect redundant calls

	void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				// TODO: dispose managed state (managed objects).
			}

			// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
			// TODO: set large fields to null.
			foreach (var data in _Data)
			{
				data.Dispose();
			}
			TopData = null;
			_Data = null;

			disposedValue = true;
		}
	}

	// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
	~InheritedDynData()
	{
		// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		Dispose(false);
	}

	// This code added to correctly implement the disposable pattern.
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		Dispose(true);
		// TODO: uncomment the following line if the finalizer is overridden above.
		GC.SuppressFinalize(this);
	}
	#endregion
}
