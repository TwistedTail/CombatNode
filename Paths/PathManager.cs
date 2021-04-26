using CombatNode.Mapping;
using CombatNode.Utilities;
using GmodNET.API;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace CombatNode.Paths
{
	public static class PathManager
	{
		private static readonly Dictionary<string, Stack<Node>> Results = new();

		public static void Load(ILua lua)
		{
			LuaStack.PushGlobalFunction(lua, "QueuePath", QueuePath);
			LuaStack.PushGlobalFunction(lua, "DiscardPath", DiscardPath);
			LuaStack.PushGlobalFunction(lua, "GetPath", GetPath);
		}

		private static int QueuePath(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!lua.IsType(3, TYPES.Vector)) { return 0; }

			string UniqueID = lua.GetString(1);
			Vector3 FromCoords = Grid.GetCoordinates(lua.GetVector(2));
			Vector3 ToCoords = Grid.GetCoordinates(lua.GetVector(3));

			if (!Grid.Nodes.TryGetValue(FromCoords, out Node From)) { return 0; }
			if (!Grid.Nodes.TryGetValue(ToCoords, out Node To)) { return 0; }

			PathFinder Finder = new();

			Task.Run(() =>
			{
				Stack<Node> Result = Finder.FindPath(From, To);

				Results.TryAdd(UniqueID, Result);
			});

			return 0;
		}

		private static int DiscardPath(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }

			Results.Remove(lua.GetString(1));

			return 0;
		}

		// TODO: Test this thing
		private static int GetPath(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }

			string UniqueID = lua.GetString(1);

			if (!Results.TryGetValue(UniqueID, out Stack<Node> Result)) { return 0; }

			lua.CreateTable();

			while (Result.Count > 0)
			{
				Node Current = Result.Pop();

				Current.PushToLua(lua);

				lua.Pop();
			}

			Results.Remove(UniqueID);

			return 1;
		}
	}
}
