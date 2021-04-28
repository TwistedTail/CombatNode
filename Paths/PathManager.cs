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

		private static bool QueuePath(Grid grid, string id, Vector3 from, Vector3 to)
		{
			if (Results.ContainsKey(id)) { return false; }
			if (!grid.Nodes.TryGetValue(from.ToString(), out Node From)) { return false; }
			if (!grid.Nodes.TryGetValue(to.ToString(), out Node To)) { return false; }

			PathFinder Finder = new(grid);

			Task.Run(() =>
			{
				Stack<Node> Result = Finder.FindPath(From, To);

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

			Grid Entry = GridManager.GetGrid(lua.GetString(1));
			
			if (Entry == null) { return 0; }

			string Identifier = lua.GetString(2);
			Vector3 From = Entry.GetCoordinates(lua.GetVector(3));
			Vector3 To = Entry.GetCoordinates(lua.GetVector(4));

			lua.PushBool(QueuePath(Entry, Identifier, From, To));

			return 1;
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

			string Identifier = lua.GetString(1);

			if (!Results.TryGetValue(Identifier, out Stack<Node> Result)) { return 0; }

			lua.CreateTable();

			while (Result.Count > 0)
			{
				Node Current = Result.Pop();

				Current.PushToLua(lua);

				lua.Pop();
			}

			Results.Remove(Identifier);

			return 1;
		}
	}
}
