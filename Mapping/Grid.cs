using GmodNET.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
		[JsonIgnore]
		public readonly HashSet<string> Locked;
		[JsonProperty]
		public readonly Dictionary<string, Node> Nodes;

		[JsonConstructor]
		public Grid(string name, Vector3 size)
		{
			Name = name;
			NodeSize = size;
			Unused = new();
			Locked = new();
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

		public bool AddNode(Vector3 coordinates, Vector3 footPos, float cost)
		{
			string Key = Node.GetKey(coordinates);

			if (Nodes.ContainsKey(Key)) { return false; }

			Node Entry = new(coordinates, footPos, cost);

			Nodes.Add(Key, Entry);
			Unused.Add(Entry);

			return true;
		}

		public Node GetNode(Vector3 coordinates)
		{
			Nodes.TryGetValue(Node.GetKey(coordinates), out Node Result);

			return Result;
		}

		public Node[] GetNodesInSphere(Vector3 center, float radius)
		{
			float RadiusSqr = radius * radius;

			return Nodes
				.Select(Entry => Entry.Value)
				.Where(Node => Vector3.DistanceSquared(Node.FootPos, center) <= RadiusSqr)
				.ToArray();
		}

		public Node[] GetConnectedNodesInRadius(Vector3 center, float radius, bool useLocked)
		{
			Vector3 Coordinates = GetCoordinates(center);
			HashSet<Node> Result = new();

			if (!Nodes.TryGetValue(Node.GetKey(Coordinates), out Node First)) { return Result.ToArray(); }

			HashSet<Node> Open = new() { First };
			Vector3 FootCenter = First.FootPos;
			float RadiusSqr = radius * radius;

			while (Open.Count > 0)
			{
				Node Current = Open.First();

				Open.Remove(Current);
				Result.Add(Current);

				foreach (var Entry in Current.Sides)
				{
					if (!Nodes.TryGetValue(Entry.Key, out Node Side)) { continue; }
					if (!useLocked & Side.Locked) { continue; }
					if (Result.Contains(Side)) { continue; }
					if (Open.Contains(Side)) { continue; }
					if (Vector3.DistanceSquared(FootCenter, Side.FootPos) > RadiusSqr) { continue; }

					Open.Add(Side);
				}
			}

			return Result.ToArray();
		}

		private void CheckHidingSpot(HashSet<Node> result, Node current, Vector3 origin)
		{
			Vector3 Direction = Vector3.Normalize(origin - current.FootPos);
			Vector3 Center = current.Coordinates;
			Vector3 Offset = Vector3.Zero;

			Direction.X = MathF.Round(Direction.X);
			Direction.Y = MathF.Round(Direction.Y);
			Direction.Z = 0f;

			for (int I = -1; I < 2; I++)
			{
				Offset.Z = I;

				string Key = Node.GetKey(Center + Offset);

				if (!Nodes.TryGetValue(Key, out Node Side)) { continue; }
				if (Side.Locked) { continue; }

				result.Add(Side);

				return;
			}
		}

		public Node[] GetHidingSpotsInRadius(Vector3 center, Vector3 origin, float radius)
		{
			Vector3 Coordinates = GetCoordinates(center);
			HashSet<Node> Result = new();

			if (!Nodes.TryGetValue(Node.GetKey(Coordinates), out Node First)) { return Result.ToArray(); }

			HashSet<Node> Open = new() { First };
			HashSet<Node> Closed = new();
			Vector3 FootCenter = First.FootPos;
			float RadiusSqr = radius * radius;

			while (Open.Count > 0)
			{
				Node Current = Open.First();

				Open.Remove(Current);
				Closed.Add(Current);

				CheckHidingSpot(Result, Current, origin);

				foreach (var Entry in Current.Sides)
				{
					if (!Nodes.TryGetValue(Entry.Key, out Node Side)) { continue; }
					if (Side.Locked) { continue; }
					if (Closed.Contains(Side)) { continue; }
					if (Open.Contains(Side)) { continue; }
					if (Vector3.DistanceSquared(FootCenter, Side.FootPos) > RadiusSqr) { continue; }

					Open.Add(Side);
				}
			}

			return Result.ToArray();
		}

		public bool LockNode(string key)
		{
			if (!Nodes.TryGetValue(key, out Node Affected)) { return false; }
			if (Affected.Locked) { return false; }

			Affected.Locked = true;

			Locked.Add(key);

			return true;
		}

		public bool UnlockNode(string key)
		{
			if (!Locked.Contains(key)) { return false; }

			Nodes.TryGetValue(key, out Node Affected);

			Affected.Locked = false;

			Locked.Remove(key);

			return true;
		}

		public bool IsConnectedTo(Vector3 from, Vector3 to)
		{
			string FromKey = Node.GetKey(from);

			if (!Nodes.TryGetValue(FromKey, out Node From)) { return false; }

			return From.Sides.ContainsKey(Node.GetKey(to));
		}

		public bool ConnectTo(Vector3 from, Vector3 to, float cost)
		{
			string FromKey = Node.GetKey(from);
			string ToKey = Node.GetKey(to);

			if (!Nodes.TryGetValue(FromKey, out Node From)) { return false; }
			if (!Nodes.TryGetValue(ToKey, out Node To)) { return false; }

			From.ConnectTo(To, cost);

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
