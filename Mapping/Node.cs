using GmodNET.API;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;

namespace CombatNode.Mapping
{
	public struct Node
	{
		[JsonProperty]
		public readonly Vector3 Coordinates;
		[JsonProperty]
		public readonly Vector3 FootPos;
		[JsonProperty]
		public readonly Dictionary<string, float> Sides;

		[JsonConstructor]
		public Node(Vector3 coords, Vector3 footPos)
		{
			Coordinates = coords;
			FootPos = footPos;
			Sides = new();
		}

		public static string GetKey(Vector3 coordinates)
		{
			return $"{coordinates.X} {coordinates.Y} {coordinates.Z}";
		}

		public string GetKey()
		{
			return $"{Coordinates.X} {Coordinates.Y} {Coordinates.Z}";
		}

		public float GetDistance(Node target)
		{
			return (FootPos - target.FootPos).Length();
		}

		public bool ConnectTo(Node target)
		{
			return Sides.TryAdd(target.GetKey(), GetDistance(target));
		}

		public bool DisconnectFrom(Node target)
		{
			return Sides.Remove(target.GetKey());
		}

		public bool RemoveFromGrid(Grid grid)
		{
			string Key = GetKey();

			if (!grid.Nodes.ContainsKey(Key)) { return false; }

			foreach (string SideKey in Sides.Keys)
			{
				if (!grid.Nodes.TryGetValue(SideKey, out Node Side)) { continue; }

				Side.DisconnectFrom(this);
			}

			Sides.Clear();

			grid.Nodes.Remove(Key);
			grid.Unused.Remove(this);

			return true;
		}

		public void PushToLua(ILua lua)
		{
			lua.CreateTable();
			lua.PushVector(Coordinates);
			lua.SetField(-2, "Coordinates");
			lua.PushVector(FootPos);
			lua.SetField(-2, "FootPos");
		}
	}
}
