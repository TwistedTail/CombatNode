using GmodNET.API;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace CombatNode
{
	public static class PathManager
	{
		private static readonly Dictionary<string, Stack<Node>> Results = new();

		public static void LoadServer(ILua lua)
		{
			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "CNode");
			lua.PushManagedFunction(QueuePath);
			lua.SetField(-2, "QueuePath");
			lua.Pop();

			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "CNode");
			lua.PushManagedFunction(DiscardPath);
			lua.SetField(-2, "DiscardPath");
			lua.Pop();

			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "CNode");
			lua.PushManagedFunction(GetPath);
			lua.SetField(-2, "GetPath");
			lua.Pop();
		}

		private static int QueuePath(ILua lua)
		{
			if (!lua.IsType(1, TYPES.STRING)) { return 0; }
			if (!lua.IsType(2, TYPES.Vector)) { return 0; }
			if (!lua.IsType(3, TYPES.Vector)) { return 0; }

			string UniqueID = lua.GetString(1);
			Vector3 FromCoords = Grid.GetCoordinates(lua.GetVector(2));
			Vector3 ToCoords = Grid.GetCoordinates(lua.GetVector(3));

			Grid.Nodes.TryGetValue(FromCoords, out Node From);
			Grid.Nodes.TryGetValue(ToCoords, out Node To);

			if (From == null) { return 0; }
			if (To == null) { return 0; }

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

			Results.TryGetValue(UniqueID, out Stack<Node> Result);

			if (Result == null) { return 0; }

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
