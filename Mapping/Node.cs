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
		public readonly float CostMult;
		[JsonProperty]
		public readonly Dictionary<string, float> Sides;
		[JsonIgnore]
		public bool Locked;

		[JsonConstructor]
		public Node(Vector3 coords, Vector3 footPos, float mult)
		{
			Coordinates = coords;
			FootPos = footPos;
			CostMult = mult;
			Sides = new();
			Locked = false;
		}

		public static string GetKey(Vector3 coordinates)
		{
			return $"{coordinates.X} {coordinates.Y} {coordinates.Z}";
		}

		public string GetKey()
		{
			return $"{Coordinates.X} {Coordinates.Y} {Coordinates.Z}";
		}

		public float GetSideCost(Node target)
		{
			return (FootPos - target.FootPos).Length() * target.CostMult;
		}

		public bool ConnectTo(Node target, float sideMult)
		{
			return Sides.TryAdd(target.GetKey(), GetSideCost(target) * sideMult);
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
			grid.Locked.Remove(Key);
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
