using GmodNET.API;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace CombatNode.Mapping
{
	public class Grid
	{
		public readonly string Name;
		public readonly Vector3 NodeSize;
		public readonly HashSet<Node> Unused;
		public readonly Dictionary<Vector3, Node> Nodes;
		public readonly Dictionary<Vector3, Sides> Connections;

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
			return Nodes.ContainsKey(coordinates);
		}

		public bool AddNode(Vector3 coordinates, Vector3 footPos)
		{
			if (Nodes.ContainsKey(coordinates)) { return false; }

			Node Entry = new(this, coordinates, footPos);

			Nodes.Add(coordinates, Entry);
			Unused.Add(Entry);

			return true;
		}

		public Node GetNode(Vector3 coordinates)
		{
			Nodes.TryGetValue(coordinates, out Node Result);

			return Result;
		}

		public bool IsConnectedTo(Vector3 from, Vector3 to)
		{
			if (!Connections.TryGetValue(from, out Sides List)) { return false; }

			return List.Contains(to);
		}

		private Sides GetOrAddSides(Vector3 key)
		{
			if (!Connections.TryGetValue(key, out Sides Result))
			{
				Result = new();

				Connections.Add(key, Result);
			}

			return Result;
		}

		public bool ConnectTo(Vector3 from, Vector3 to)
		{
			if (!Nodes.TryGetValue(from, out Node From)) { return false; }
			if (!Nodes.TryGetValue(to, out Node To)) { return false; }

			Sides FromSides = GetOrAddSides(from);
			Sides ToSides = GetOrAddSides(to);
			float Cost = GetSideCost(From, To);

			FromSides.Add(to, Cost);
			ToSides.Add(from, Cost);

			Unused.Remove(From);
			Unused.Remove(To);

			return true;
		}

		public bool DisconnectFrom(Vector3 from, Vector3 to)
		{
			if (!Nodes.TryGetValue(from, out Node From)) { return false; }
			if (!Nodes.TryGetValue(to, out Node To)) { return false; }

			Sides FromSides = GetOrAddSides(from);
			Sides ToSides = GetOrAddSides(to);

			FromSides.Remove(to);
			ToSides.Remove(from);

			if (FromSides.Count == 0) { Unused.Add(From); }
			if (ToSides.Count == 0) { Unused.Add(To); }

			return true;
		}

		public bool RemoveNode(Vector3 coordinates)
		{
			if (!Nodes.TryGetValue(coordinates, out Node Entry)) { return false; }

			Nodes.Remove(coordinates);
			Connections.Remove(coordinates);
			Unused.Remove(Entry);

			return true;
		}

		public int ClearNodes()
		{
			int Count = Nodes.Count;

			foreach (Node Current in Nodes.Values)
			{
				Connections.Remove(Current.Coordinates);
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
				Vector3 Coordinates = Current.Coordinates;

				Nodes.Remove(Coordinates);
				Connections.Remove(Coordinates);
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
