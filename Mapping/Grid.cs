using CombatNode.Utilities;
using GmodNET.API;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace CombatNode.Mapping
{
	public static class Grid
	{
		public static readonly Dictionary<Vector3, Node> Nodes = new();
		public static readonly Vector3 NodeSize = new(35, 35, 75); // Roughly the same size as a standing player

		public static void Load(ILua lua)
		{
			LuaStack.PushVector(lua, "NodeSize", NodeSize);

			LuaStack.PushFunction(lua, "GetCoordinates", GetCoordinates);
			LuaStack.PushFunction(lua, "GetRoundedPos", GetRoundedPos);
			LuaStack.PushFunction(lua, "GetNodeCount", GetNodeCount);
			LuaStack.PushFunction(lua, "GetNode", GetNode);

			LuaStack.PushFunction(lua, "HasNode", HasNode);
			LuaStack.PushFunction(lua, "AddNode", AddNode);
			LuaStack.PushFunction(lua, "ConnectNodes", ConnectNodes);
			LuaStack.PushFunction(lua, "DisconnectNodes", DisconnectNodes);
			LuaStack.PushFunction(lua, "RemoveNode", RemoveNode);
			LuaStack.PushFunction(lua, "RemoveAllNodes", RemoveAllNodes);
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
