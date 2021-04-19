using GmodNET.API;
using System;
using System.Numerics;

namespace CombatNode.Utilities
{
	public static class LuaStack
	{
		public static void PushFunction(ILua lua, string key, Func<ILua, int> value)
		{
			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "CNode");
			lua.PushManagedFunction(value);
			lua.SetField(-2, key);
			lua.Pop();
		}

		public static void PushVector(ILua lua, string key, Vector3 value)
		{
			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "CNode");
			lua.PushVector(value);
			lua.SetField(-2, key);
			lua.Pop();
		}
	}
}
