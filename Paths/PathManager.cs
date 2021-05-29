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
			LuaStack.PushGlobalFunction(lua, "GetPaths", GetPaths);
		}

		private static void StackToLua(ILua lua, Stack<Node> result)
		{
			lua.CreateTable();

			int Index = 0;

			foreach (Node Entry in result)
			{
				Index++;

				lua.PushNumber(Index);
				lua.PushVector(Entry.FootPos);
				lua.SetTable(-3);
			}

			result.Clear();
		}

		private static bool QueuePath(Grid grid, string id, Vector3 from, Vector3 to)
		{
			if (Results.ContainsKey(id)) { return false; }
			if (!grid.Nodes.TryGetValue(Node.GetKey(from), out Node From)) { return false; }
			if (!grid.Nodes.TryGetValue(Node.GetKey(to), out Node To)) { return false; }

			PathFinder Finder = new(grid, From, To);

			Task.Run(() =>
			{
				Stack<Node> Result = Finder.FindPath();

				Results.TryAdd(id, Result);
			});

			return true;
		}

		private static int QueuePath(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.STRING)) { return 0; }
			if (!lua.IsType(3, TYPES.Vector)) { return 0; }
			if (!lua.IsType(4, TYPES.Vector)) { return 0; }

			string GridName = lua.GetString(1);

			if (!GridManager.HasGrid(GridName)) { return 0; }

			string Identifier = lua.GetString(2);
			Grid Entry = GridManager.GetGrid(GridName);
			Vector3 From = Entry.GetCoordinates(lua.GetVector(3));
			Vector3 To = Entry.GetCoordinates(lua.GetVector(4));

			lua.PushBool(QueuePath(Entry, Identifier, From, To));

			return 1;
		}

		private static int GetPaths(ILua lua)
		{
			if (Results.Count == 0) { return 0; }

			lua.CreateTable();

			foreach (var Entry in Results)
			{
				lua.PushString(Entry.Key);

				StackToLua(lua, Entry.Value);

				lua.SetTable(-3);
			}

			Results.Clear();

			return 1;
		}
	}
}
