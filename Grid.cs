using GmodNET.API;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace CombatNode
{
	public static class Grid
	{
		public static readonly Dictionary<Vector3, Node> Nodes = new();
		public static readonly Vector3 NodeSize = new(35, 35, 75); // Roughly the same size as a standing player

		public static void LoadServer(ILua lua)
		{
			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "CNode");
			lua.PushManagedFunction(AddNode);
			lua.SetField(-2, "AddNode");
			lua.Pop();

			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "CNode");
			lua.PushManagedFunction(ConnectNodes);
			lua.SetField(-2, "ConnectNodes");
			lua.Pop();

			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "CNode");
			lua.PushManagedFunction(DisconnectNodes);
			lua.SetField(-2, "DisconnectNodes");
			lua.Pop();

			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "CNode");
			lua.PushManagedFunction(RemoveNode);
			lua.SetField(-2, "RemoveNode");
			lua.Pop();

			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "CNode");
			lua.PushManagedFunction(RemoveAllNodes);
			lua.SetField(-2, "RemoveAllNodes");
			lua.Pop();
		}

		public static void LoadShared(ILua lua)
		{
			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "CNode");
			lua.PushVector(NodeSize);
			lua.SetField(-2, "NodeSize");
			lua.Pop();

			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "CNode");
			lua.PushManagedFunction(GetCoordinates);
			lua.SetField(-2, "GetCoordinates");
			lua.Pop();

			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "CNode");
			lua.PushManagedFunction(GetRoundedPos);
			lua.SetField(-2, "GetRoundedPos");
			lua.Pop();

			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "CNode");
			lua.PushManagedFunction(GetNodeCount);
			lua.SetField(-2, "GetNodeCount");
			lua.Pop();

			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "CNode");
			lua.PushManagedFunction(HasNode);
			lua.SetField(-2, "HasNode");
			lua.Pop();

			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "CNode");
			lua.PushManagedFunction(GetNode);
			lua.SetField(-2, "GetNode");
			lua.Pop();
		}

		public static Vector3 GetCoordinates(Vector3 coords)
		{
			float X = MathF.Round(coords.X / NodeSize.X);
			float Y = MathF.Round(coords.Y / NodeSize.Y);
			float Z = MathF.Round(coords.Z / NodeSize.Z);

			return new Vector3(X, Y, Z);
		}

		private static int GetCoordinates(ILua lua)
		{
			if (!lua.IsType(1, TYPES.Vector)) { return 0; }

			Vector3 Coordinates = GetCoordinates(lua.GetVector(1));

			lua.PushVector(Coordinates);

			return 1;
		}

		private static int GetRoundedPos(ILua lua)
		{
			if (!lua.IsType(1, TYPES.Vector)) { return 0; }

			Vector3 Coordinates = GetCoordinates(lua.GetVector(1));

			lua.PushVector(Coordinates * NodeSize);

			return 1;
		}

		private static int GetNodeCount(ILua lua)
		{
			lua.PushNumber(Nodes.Count);

			return 1;
		}

		private static int HasNode(ILua lua)
		{
			if (!lua.IsType(1, TYPES.Vector)) { return 0; }

			Vector3 Coordinates = GetCoordinates(lua.GetVector(1));

			lua.PushBool(Nodes.ContainsKey(Coordinates));

			return 1;
		}

		private static int AddNode(ILua lua)
		{
			if (!lua.IsType(1, TYPES.Vector)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }

			Vector3 Coordinates = GetCoordinates(lua.GetVector(1));
			Node NewNode = new(Coordinates, lua.GetVector(2));

			lua.PushBool(Nodes.TryAdd(Coordinates, NewNode));

			return 1;
		}

		private static int GetNode(ILua lua)
		{
			if (!lua.IsType(1, TYPES.Vector)) { return 0; }

			Vector3 Coordinates = GetCoordinates(lua.GetVector(1));

			if (!Nodes.TryGetValue(Coordinates, out Node Result)) { return 0; }

			Result.PushToLua(lua);

			return 1; // NOTE: I wonder what happens if we do this and we haven't pushed anything
		}

		private static int ConnectNodes(ILua lua)
		{
			if (!lua.IsType(1, TYPES.Vector)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }

			Vector3 FromCoords = GetCoordinates(lua.GetVector(1));
			Vector3 ToCoords = GetCoordinates(lua.GetVector(2));

			if (FromCoords.Equals(ToCoords)) { return 0; }
			if (!Nodes.TryGetValue(FromCoords, out Node From)) { return 0; }
			if (!Nodes.TryGetValue(ToCoords, out Node To)) { return 0; }

			From.Connect(To);
			To.Connect(From);

			return 0;
		}

		private static int DisconnectNodes(ILua lua)
		{
			if (!lua.IsType(1, TYPES.Vector)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }

			Vector3 FromCoords = GetCoordinates(lua.GetVector(1));
			Vector3 ToCoords = GetCoordinates(lua.GetVector(2));

			if (FromCoords.Equals(ToCoords)) { return 0; }
			if (!Nodes.TryGetValue(FromCoords, out Node From)) { return 0; }
			if (!Nodes.TryGetValue(ToCoords, out Node To)) { return 0; }

			From.Disconnect(To);
			To.Disconnect(From);

			return 0;
		}

		private static int RemoveNode(ILua lua)
		{
			if (!lua.IsType(1, TYPES.Vector)) { return 0; }

			Vector3 Coordinates = GetCoordinates(lua.GetVector(1));

			if (!Nodes.TryGetValue(Coordinates, out Node Target)) { return 0; }

			Target.Remove();
			Nodes.Remove(Coordinates);

			return 0;
		}

		private static int RemoveAllNodes(ILua lua)
		{
			int Count = Nodes.Count;

			foreach (Node Value in Nodes.Values)
			{
				Value.Remove();
			}

			Nodes.Clear();

			lua.PushNumber(Count);

			return 1;
		}
	}
}
