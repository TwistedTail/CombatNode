using GmodNET.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace CombatNode.Mapping
{
	public class Grid
	{
		[JsonProperty]
		public readonly string Name;
		[JsonProperty]
		public readonly Vector3 NodeSize;
		[JsonIgnore]
		public readonly HashSet<Node> Unused;
		[JsonProperty]
		public readonly Dictionary<string, Node> Nodes;
		[JsonProperty]
		public readonly Dictionary<string, Sides> Connections;

		[JsonConstructor]
		public Grid(string name, Vector3 size)
		{
			Name = name;
			NodeSize = size;
			Unused = new();
			Nodes = new();
			Connections = new();
		}

		public Vector3 GetCoordinates(Vector3 position)
		{
			float X = MathF.Round(position.X / NodeSize.X);
			float Y = MathF.Round(position.Y / NodeSize.Y);
			float Z = MathF.Round(position.Z / NodeSize.Z);

			return new Vector3(X, Y, Z);
		}

		public Vector3 RoundPosition(Vector3 position)
		{
			return GetCoordinates(position) * NodeSize;
		}

		public float GetSideCost(Node from, Node to)
		{
			return (from.FootPos - to.FootPos).Length();
		}

		public bool HasNode(Vector3 coordinates)
		{
			return Nodes.ContainsKey(coordinates.ToString());
		}

		public bool AddNode(Vector3 coordinates, Vector3 footPos)
		{
			string Key = coordinates.ToString();

			if (Nodes.ContainsKey(Key)) { return false; }

			Node Entry = new(NodeSize, coordinates, footPos);

			Nodes.Add(Key, Entry);
			Unused.Add(Entry);

			return true;
		}

		public Node GetNode(Vector3 coordinates)
		{
			Nodes.TryGetValue(coordinates.ToString(), out Node Result);

			return Result;
		}

		public bool IsConnectedTo(Vector3 from, Vector3 to)
		{
			if (!Connections.TryGetValue(from.ToString(), out Sides List)) { return false; }

			return List.Contains(to.ToString());
		}

		private Sides GetOrAddSides(Vector3 coordinates)
		{
			string Key = coordinates.ToString();

			if (!Connections.TryGetValue(Key, out Sides Result))
			{
				Result = new();

				Connections.Add(Key, Result);
			}

			return Result;
		}

		public bool ConnectTo(Vector3 from, Vector3 to)
		{
			string FromKey = from.ToString();
			string ToKey = to.ToString();

			if (!Nodes.TryGetValue(FromKey, out Node From)) { return false; }
			if (!Nodes.TryGetValue(ToKey, out Node To)) { return false; }

			Sides FromSides = GetOrAddSides(from);
			Sides ToSides = GetOrAddSides(to);
			float Cost = GetSideCost(From, To);

			FromSides.Add(ToKey, Cost);
			ToSides.Add(FromKey, Cost);

			Unused.Remove(From);
			Unused.Remove(To);

			return true;
		}

		public bool DisconnectFrom(Vector3 from, Vector3 to)
		{
			string FromKey = from.ToString();
			string ToKey = to.ToString();

			if (!Nodes.TryGetValue(FromKey, out Node From)) { return false; }
			if (!Nodes.TryGetValue(ToKey, out Node To)) { return false; }

			Sides FromSides = GetOrAddSides(from);
			Sides ToSides = GetOrAddSides(to);

			FromSides.Remove(ToKey);
			ToSides.Remove(FromKey);

			if (FromSides.Count == 0) { Unused.Add(From); }
			if (ToSides.Count == 0) { Unused.Add(To); }

			return true;
		}

		public bool RemoveNode(Vector3 coordinates)
		{
			string Key = coordinates.ToString();

			if (!Nodes.TryGetValue(Key, out Node Entry)) { return false; }

			Nodes.Remove(Key);
			Connections.Remove(Key);
			Unused.Remove(Entry);

			return true;
		}

		public int ClearNodes()
		{
			int Count = Nodes.Count;

			foreach (Node Current in Nodes.Values)
			{
				Connections.Remove(Current.Coordinates.ToString());
			}

			Nodes.Clear();
			Unused.Clear();

			return Count;
		}

		public int PurgeUnused()
		{
			int Count = Unused.Count;

			foreach (Node Current in Unused)
			{
				string Key = Current.Coordinates.ToString();

				Nodes.Remove(Key);
				Connections.Remove(Key);
			}

			Unused.Clear();

			return Count;
		}

		public void PushToLua(ILua lua)
		{
			lua.CreateTable();
			lua.PushString(Name);
			lua.SetField(-2, "Name");
			lua.PushVector(NodeSize);
			lua.SetField(-2, "NodeSize");
		}
	}
}
