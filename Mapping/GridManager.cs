using CombatNode.Utilities;
using GmodNET.API;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Numerics;

namespace CombatNode.Mapping
{
	public static class GridManager
	{
		private static readonly Dictionary<string, Grid> Grids = new();

		public static void Load(ILua lua)
		{
			// Grid functions
			LuaStack.PushGlobalFunction(lua, "AddGrid", AddGrid);
			LuaStack.PushGlobalFunction(lua, "HasGrid", HasGrid);
			LuaStack.PushGlobalFunction(lua, "GetGrid", GetGrid);
			LuaStack.PushGlobalFunction(lua, "RemoveGrid", RemoveGrid);
			LuaStack.PushGlobalFunction(lua, "SerializeGrid", SerializeGrid);
			LuaStack.PushGlobalFunction(lua, "DeserializeGrid", DeserializeGrid);

			// Grid-related functions
			LuaStack.PushGlobalFunction(lua, "GetNodeSize", GetNodeSize);
			LuaStack.PushGlobalFunction(lua, "GetCoordinates", GetCoordinates);
			LuaStack.PushGlobalFunction(lua, "GetRoundedPos", GetRoundedPos);
			LuaStack.PushGlobalFunction(lua, "GetUnusedCount", GetUnusedCount);
			LuaStack.PushGlobalFunction(lua, "GetNodeCount", GetNodeCount);
			LuaStack.PushGlobalFunction(lua, "GetNodeList", GetNodeList);
			LuaStack.PushGlobalFunction(lua, "GetNodesInSphere", GetNodesInSphere);
			LuaStack.PushGlobalFunction(lua, "GetConnectedNodesInRadius", GetConnectedNodesInRadius);

			// Node functions
			LuaStack.PushGlobalFunction(lua, "HasNode", HasNode);
			LuaStack.PushGlobalFunction(lua, "AddNode", AddNode);
			LuaStack.PushGlobalFunction(lua, "GetNode", GetNode);
			LuaStack.PushGlobalFunction(lua, "LockNode", LockNode);
			LuaStack.PushGlobalFunction(lua, "UnlockNode", UnlockNode);
			LuaStack.PushGlobalFunction(lua, "IsConnectedTo", IsConnectedTo);
			LuaStack.PushGlobalFunction(lua, "ConnectTo", ConnectTo);
			LuaStack.PushGlobalFunction(lua, "DisconnectFrom", DisconnectFrom);
			LuaStack.PushGlobalFunction(lua, "RemoveNode", RemoveNode);
			LuaStack.PushGlobalFunction(lua, "ClearNodes", ClearNodes);
			LuaStack.PushGlobalFunction(lua, "PurgeUnused", PurgeUnused);
		}

		public static bool HasGrid(string name)
		{
			return Grids.ContainsKey(name);
		}

		private static int HasGrid(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }

			lua.PushBool(HasGrid(lua.GetString(1)));

			return 1;
		}

		private static int AddGrid(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }

			string Name = lua.GetString(1);

			if (HasGrid(Name)) { return 0; }

			Vector3 Size = lua.GetVector(2);
			Grid NewGrid = new(Name, Size);

			lua.PushBool(Grids.TryAdd(Name, NewGrid));

			return 1;
		}

		public static Grid GetGrid(string name)
		{
			Grids.TryGetValue(name, out Grid Result);

			return Result;
		}

		private static int GetGrid(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			Map.PushToLua(lua);

			return 1;
		}

		private static int RemoveGrid(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }

			lua.PushBool(Grids.Remove(lua.GetString(1)));

			return 1;
		}

		private static int SerializeGrid(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			lua.PushString(JsonConvert.SerializeObject(Map, Formatting.Indented));

			return 1;
		}

		private static int DeserializeGrid(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }

			string Json = lua.GetString(1);
			Grid? Result = JsonConvert.DeserializeObject<Grid>(Json);

			if (Result.HasValue)
			{
				Grid New = Result.Value;
				string Name = New.Name;

				Grids.Remove(Name);
				Grids.TryAdd(Name, New);
			}

			lua.PushBool(Result.HasValue);

			return 1;
		}

		private static int GetNodeSize(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			lua.PushVector(Map.NodeSize);

			return 1;
		}

		private static int GetNodesInSphere(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!lua.IsType(3, TYPES.NUMBER)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			Vector3 Center = Map.RoundPosition(lua.GetVector(2));
			float Radius = (float)lua.GetNumber(3);
			Node[] Nodes = Map.GetNodesInSphere(Center, Radius);
			int Index = 0;

			lua.CreateTable();

			foreach (Node Current in Nodes)
			{
				Index++;

				lua.PushNumber(Index);

				Current.PushToLua(lua);

				lua.SetTable(-3);
			}

			return 1;
		}

		private static int GetConnectedNodesInRadius(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!lua.IsType(3, TYPES.NUMBER)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			Vector3 Center = lua.GetVector(2);
			float Radius = (float)lua.GetNumber(3);
			bool UseLocked = lua.IsType(4, TYPES.BOOL) ? lua.GetBool(4) : true;
			Node[] Nodes = Map.GetConnectedNodesInRadius(Center, Radius, UseLocked);
			int Index = 0;

			lua.CreateTable();

			foreach (Node Current in Nodes)
			{
				Index++;

				lua.PushNumber(Index);

				Current.PushToLua(lua);

				lua.SetTable(-3);
			}

			return 1;
		}

		private static int GetCoordinates(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			Vector3 Coordinates = Map.GetCoordinates(lua.GetVector(2));

			lua.PushVector(Coordinates);

			return 1;
		}

		private static int GetRoundedPos(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			Vector3 Position = Map.RoundPosition(lua.GetVector(2));

			lua.PushVector(Position);

			return 1;
		}

		private static int GetUnusedCount(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			lua.PushNumber(Map.Unused.Count);

			return 1;
		}

		private static int GetNodeCount(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			lua.PushNumber(Map.Nodes.Count);

			return 1;
		}

		private static int GetNodeList(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			int Index = 0;

			lua.CreateTable();

			foreach (Node Current in Map.Nodes.Values)
			{
				Index++;

				lua.PushNumber(Index);

				Current.PushToLua(lua);

				lua.SetTable(-3);
			}

			return 1;
		}

		private static int HasNode(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			Vector3 Coordinates = Map.GetCoordinates(lua.GetVector(2));

			lua.PushBool(Map.HasNode(Coordinates));

			return 1;
		}

		private static int AddNode(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!lua.IsType(3, TYPES.Vector)) { return 0; }
			if (!lua.IsType(4, TYPES.NUMBER)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			Vector3 Coordinates = Map.GetCoordinates(lua.GetVector(2));
			float Cost = (float)lua.GetNumber(4);

			lua.PushBool(Map.AddNode(Coordinates, lua.GetVector(3), Cost));

			return 1;
		}

		private static int GetNode(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			Vector3 Coordinates = Map.GetCoordinates(lua.GetVector(2));

			if (!Map.HasNode(Coordinates)) { return 0; }

			Node Result = Map.GetNode(Coordinates);

			Result.PushToLua(lua);

			return 1;
		}

		private static int LockNode(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			Vector3 Coordinates = Map.GetCoordinates(lua.GetVector(2));
			string Key = Node.GetKey(Coordinates);

			lua.PushBool(Map.LockNode(Key));

			return 1;
		}

		private static int UnlockNode(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			Vector3 Coordinates = Map.GetCoordinates(lua.GetVector(2));
			string Key = Node.GetKey(Coordinates);

			lua.PushBool(Map.UnlockNode(Key));

			return 1;
		}

		private static int IsConnectedTo(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!lua.IsType(3, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			Vector3 From = Map.GetCoordinates(lua.GetVector(2));
			Vector3 To = Map.GetCoordinates(lua.GetVector(3));

			lua.PushBool(Map.IsConnectedTo(From, To));

			return 1;
		}

		private static int ConnectTo(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!lua.IsType(3, TYPES.Vector)) { return 0; }
			if (!lua.IsType(4, TYPES.NUMBER)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			Vector3 From = Map.GetCoordinates(lua.GetVector(2));
			Vector3 To = Map.GetCoordinates(lua.GetVector(3));
			float Cost = (float)lua.GetNumber(4);

			lua.PushBool(Map.ConnectTo(From, To, Cost));

			return 1;
		}

		private static int DisconnectFrom(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!lua.IsType(3, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			Vector3 From = Map.GetCoordinates(lua.GetVector(2));
			Vector3 To = Map.GetCoordinates(lua.GetVector(3));

			lua.PushBool(Map.DisconnectFrom(From, To));

			return 1;
		}

		private static int RemoveNode(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			Vector3 Coordinates = Map.GetCoordinates(lua.GetVector(2));

			lua.PushBool(Map.RemoveNode(Coordinates));

			return 0;
		}

		private static int ClearNodes(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			lua.PushNumber(Map.ClearNodes());

			return 1;
		}

		private static int PurgeUnused(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Map)) { return 0; }

			lua.PushNumber(Map.PurgeUnused());

			return 1;
		}
	}
}
