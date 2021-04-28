using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;

namespace CombatNode.Mapping
{
	public class Sides
	{
		[JsonProperty]
		public readonly Dictionary<string, float> Connections;
		[JsonIgnore]
		public int Count => Connections.Count;

		[JsonConstructor]
		public Sides()
		{
			Connections = new();
		}

		public bool Add(string key, float value)
		{
			return Connections.TryAdd(key, value);
		}

		public bool Contains(string key)
		{
			return Connections.ContainsKey(key);
		}

		public bool Remove(string key)
		{
			return Connections.Remove(key);
		}
	}
}
