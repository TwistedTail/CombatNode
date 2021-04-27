using System.Collections.Generic;
using System.Numerics;

namespace CombatNode.Mapping
{
	public class Sides
	{
		public readonly Dictionary<Vector3, float> Connections;
		public int Count => Connections.Count;

		public Sides()
		{
			Connections = new();
		}

		public bool Add(Vector3 key, float value)
		{
			return Connections.TryAdd(key, value);
		}

		public bool Contains(Vector3 key)
		{
			return Connections.ContainsKey(key);
		}

		public bool Remove(Vector3 key)
		{
			return Connections.Remove(key);
		}
	}
}
