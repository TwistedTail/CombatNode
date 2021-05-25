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

			// Node functions
			LuaStack.PushGlobalFunction(lua, "HasNode", HasNode);
			LuaStack.PushGlobalFunction(lua, "AddNode", AddNode);
			LuaStack.PushGlobalFunction(lua, "GetNode", GetNode);
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
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			Entry.PushToLua(lua);

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
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			lua.PushString(JsonConvert.SerializeObject(Entry, Formatting.Indented));

			return 1;
		}

		private static int DeserializeGrid(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }

			string Json = lua.GetString(1);
			Grid? Result = JsonConvert.DeserializeObject<Grid>(Json);
			bool Success = Result.HasValue;

			if (Result.HasValue)
			{
				Grid New = Result.Value;
				string Name = New.Name;

				Grids.Remove(Name);
				Grids.TryAdd(Name, New);
			}

			lua.PushBool(Success);

			return 1;
		}

		private static int GetNodeSize(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			lua.PushVector(Entry.NodeSize);

			return 1;
		}

		private static int GetCoordinates(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			Vector3 Coordinates = Entry.GetCoordinates(lua.GetVector(2));

			lua.PushVector(Coordinates);

			return 1;
		}

		private static int GetRoundedPos(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			Vector3 Position = Entry.RoundPosition(lua.GetVector(2));

			lua.PushVector(Position);

			return 1;
		}

		private static int GetUnusedCount(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			lua.PushNumber(Entry.Unused.Count);

			return 1;
		}

		private static int GetNodeCount(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			lua.PushNumber(Entry.Nodes.Count);

			return 1;
		}

		private static int GetNodeList(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			int Index = 0;

			lua.CreateTable();

			foreach (Node Current in Entry.Nodes.Values)
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
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			Vector3 Coordinates = Entry.GetCoordinates(lua.GetVector(2));

			lua.PushBool(Entry.HasNode(Coordinates));

			return 1;
		}

		private static int AddNode(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!lua.IsType(3, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			Vector3 Coordinates = Entry.GetCoordinates(lua.GetVector(2));

			lua.PushBool(Entry.AddNode(Coordinates, lua.GetVector(3)));

			return 1;
		}

		private static int GetNode(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			Vector3 Coordinates = Entry.GetCoordinates(lua.GetVector(2));

			if (!Entry.HasNode(Coordinates)) { return 0; }

			Node Result = Entry.GetNode(Coordinates);

			Result.PushToLua(lua);

			return 1;
		}

		private static int IsConnectedTo(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!lua.IsType(3, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			Vector3 From = Entry.GetCoordinates(lua.GetVector(2));
			Vector3 To = Entry.GetCoordinates(lua.GetVector(3));

			lua.PushBool(Entry.IsConnectedTo(From, To));

			return 1;
		}

		private static int ConnectTo(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!lua.IsType(3, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			Vector3 From = Entry.GetCoordinates(lua.GetVector(2));
			Vector3 To = Entry.GetCoordinates(lua.GetVector(3));

			lua.PushBool(Entry.ConnectTo(From, To));

			return 1;
		}

		private static int DisconnectFrom(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!lua.IsType(3, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			Vector3 From = Entry.GetCoordinates(lua.GetVector(2));
			Vector3 To = Entry.GetCoordinates(lua.GetVector(3));

			lua.PushBool(Entry.DisconnectFrom(From, To));

			return 1;
		}

		private static int RemoveNode(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			Vector3 Coordinates = Entry.GetCoordinates(lua.GetVector(2));

			lua.PushBool(Entry.RemoveNode(Coordinates));

			return 0;
		}

		private static int ClearNodes(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			lua.PushNumber(Entry.ClearNodes());

			return 1;
		}

		private static int PurgeUnused(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!Grids.TryGetValue(lua.GetString(1), out Grid Entry)) { return 0; }

			lua.PushNumber(Entry.PurgeUnused());

			return 1;
		}
	}
}
