using CombatNode.Utilities;
using GmodNET.API;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace CombatNode.Mapping
{
	public static class Grid
	{
		private static readonly HashSet<Node> Unused = new();
		public static readonly Dictionary<Vector3, Node> Nodes = new();
		public static readonly Vector3 NodeSize = new(35, 35, 75); // Roughly the same size as a standing player

		public static void Load(ILua lua)
		{
			LuaStack.PushGlobalVector(lua, "NodeSize", NodeSize);

			LuaStack.PushGlobalFunction(lua, "GetCoordinates", GetCoordinates);
			LuaStack.PushGlobalFunction(lua, "GetRoundedPos", GetRoundedPos);
			LuaStack.PushGlobalFunction(lua, "GetUnusedCount", GetUnusedCount);
			LuaStack.PushGlobalFunction(lua, "GetNodeCount", GetNodeCount);
			LuaStack.PushGlobalFunction(lua, "GetNodeList", GetNodeList);
			LuaStack.PushGlobalFunction(lua, "GetNode", GetNode);

			LuaStack.PushGlobalFunction(lua, "HasNode", HasNode);
			LuaStack.PushGlobalFunction(lua, "AddNode", AddNode);
			LuaStack.PushGlobalFunction(lua, "IsConnectedTo", IsConnectedTo);
			LuaStack.PushGlobalFunction(lua, "ConnectTo", ConnectTo);
			LuaStack.PushGlobalFunction(lua, "DisconnectFrom", DisconnectFrom);
			LuaStack.PushGlobalFunction(lua, "RemoveNode", RemoveNode);
			LuaStack.PushGlobalFunction(lua, "ClearNodes", ClearNodes);
			LuaStack.PushGlobalFunction(lua, "PurgeUnused", PurgeUnused);
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

		private static int GetUnusedCount(ILua lua)
		{
			lua.PushNumber(Unused.Count);

			return 1;
		}

		private static int GetNodeCount(ILua lua)
		{
			lua.PushNumber(Nodes.Count);

			return 1;
		}

		private static int GetNodeList(ILua lua)
		{
			int Index = 0;

			lua.CreateTable();

			foreach (Node Current in Nodes.Values)
			{
				Index++;

				lua.PushNumber(Index);

				lua.CreateTable();
				lua.PushVector(Current.Position);
				lua.SetField(-2, "Position");
				lua.PushVector(Current.FootPos);
				lua.SetField(-2, "FootPos");

				lua.SetTable(-3);
			}

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

			if (Nodes.ContainsKey(Coordinates)) { return 0; }

			Node Entry = new(Coordinates, lua.GetVector(2));

			Nodes.Add(Coordinates, Entry);
			Unused.Add(Entry);

			lua.PushBool(true);

			return 1;
		}

		private static int GetNode(ILua lua)
		{
			if (!lua.IsType(1, TYPES.Vector)) { return 0; }

			Vector3 Coordinates = GetCoordinates(lua.GetVector(1));

			if (!Nodes.TryGetValue(Coordinates, out Node Result)) { return 0; }

			Result.PushToLua(lua);

			return 1;
		}

		private static int IsConnectedTo(ILua lua)
		{
			if (!lua.IsType(1, TYPES.Vector)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }

			Vector3 FromCoords = GetCoordinates(lua.GetVector(1));
			Vector3 ToCoords = GetCoordinates(lua.GetVector(2));

			if (FromCoords.Equals(ToCoords)) { return 0; }
			if (!Nodes.TryGetValue(FromCoords, out Node From)) { return 0; }
			if (!Nodes.TryGetValue(ToCoords, out Node To)) { return 0; }

			lua.PushBool(From.Sides.ContainsKey(To));

			return 1;
		}

		private static int ConnectTo(ILua lua)
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

			Unused.Remove(From);
			Unused.Remove(To);

			return 0;
		}

		private static int DisconnectFrom(ILua lua)
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

			if (From.Sides.Count == 0) { Unused.Add(From); }
			if (To.Sides.Count == 0) { Unused.Add(To); }

			return 0;
		}

		private static int RemoveNode(ILua lua)
		{
			if (!lua.IsType(1, TYPES.Vector)) { return 0; }

			Vector3 Coordinates = GetCoordinates(lua.GetVector(1));

			if (!Nodes.TryGetValue(Coordinates, out Node Target)) { return 0; }

			Target.Remove();
			Unused.Remove(Target);
			Nodes.Remove(Coordinates);

			return 0;
		}

		private static int ClearNodes(ILua lua)
		{
			lua.PushNumber(Nodes.Count);

			foreach (Node Value in Nodes.Values)
			{
				Value.Remove();
			}

			Nodes.Clear();
			Unused.Clear();

			return 1;
		}

		private static int PurgeUnused(ILua lua)
		{
			lua.PushNumber(Unused.Count);

			foreach (Node Current in Unused)
			{
				Current.Remove();
				Nodes.Remove(Current.Coordinates);
			}

			Unused.Clear();

			return 1;
		}
	}
}
