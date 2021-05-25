using GmodNET.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace CombatNode.Mapping
{
	public struct Grid
	{
		[JsonProperty]
		public readonly string Name;
		[JsonProperty]
		public readonly Vector3 NodeSize;
		[JsonIgnore]
		public readonly HashSet<Node> Unused;
		[JsonProperty]
		public readonly Dictionary<string, Node> Nodes;

		[JsonConstructor]
		public Grid(string name, Vector3 size)
		{
			Name = name;
			NodeSize = size;
			Unused = new();
			Nodes = new();
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

		public bool HasNode(Vector3 coordinates)
		{
			return Nodes.ContainsKey(Node.GetKey(coordinates));
		}

		public bool AddNode(Vector3 coordinates, Vector3 footPos)
		{
			string Key = Node.GetKey(coordinates);

			if (Nodes.ContainsKey(Key)) { return false; }

			Node Entry = new(coordinates, footPos);

			Nodes.Add(Key, Entry);
			Unused.Add(Entry);

			return true;
		}

		public Node GetNode(Vector3 coordinates)
		{
			Nodes.TryGetValue(Node.GetKey(coordinates), out Node Result);

			return Result;
		}

		public bool IsConnectedTo(Vector3 from, Vector3 to)
		{
			string FromKey = Node.GetKey(from);

			if (!Nodes.TryGetValue(FromKey, out Node From)) { return false; }

			return From.Sides.ContainsKey(Node.GetKey(to));
		}

		public bool ConnectTo(Vector3 from, Vector3 to)
		{
			string FromKey = Node.GetKey(from);
			string ToKey = Node.GetKey(to);

			if (!Nodes.TryGetValue(FromKey, out Node From)) { return false; }
			if (!Nodes.TryGetValue(ToKey, out Node To)) { return false; }

			From.ConnectTo(To);
			To.ConnectTo(From);

			Unused.Remove(From);
			Unused.Remove(To);

			return true;
		}

		public bool DisconnectFrom(Vector3 from, Vector3 to)
		{
			string FromKey = Node.GetKey(from);
			string ToKey = Node.GetKey(to);

			if (!Nodes.TryGetValue(FromKey, out Node From)) { return false; }
			if (!Nodes.TryGetValue(ToKey, out Node To)) { return false; }

			From.DisconnectFrom(To);
			To.DisconnectFrom(From);

			if (From.Sides.Count == 0) { Unused.Add(From); }
			if (To.Sides.Count == 0) { Unused.Add(To); }

			return true;
		}

		public bool RemoveNode(Vector3 coordinates)
		{
			string Key = Node.GetKey(coordinates);

			if (!Nodes.TryGetValue(Key, out Node Entry)) { return false; }

			Entry.RemoveFromGrid(this);

			return true;
		}

		public int ClearNodes()
		{
			int Count = Nodes.Count;

			foreach (Node Current in Nodes.Values)
			{
				Current.RemoveFromGrid(this);
			}

			return Count;
		}

		public int PurgeUnused()
		{
			int Count = Unused.Count;

			foreach (Node Current in Unused)
			{
				Current.RemoveFromGrid(this);
			}

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
