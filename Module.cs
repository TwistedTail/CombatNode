using System;
using System.Runtime.InteropServices;
using GmodNET.API;

namespace CombatNode
{
	public class Module : IModule
	{
		public string ModuleName => "Combat Node";
		public string ModuleVersion => "0.1";

		public void Load(ILua lua, bool is_serverside, ModuleAssemblyLoadContext assembly_context)
		{
			// Creating global table
			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.CreateTable();
			lua.SetField(-2, "CNode");
			lua.Pop(1);

			if (is_serverside)
			{
				PathManager.LoadServer(lua);
				Grid.LoadServer(lua);
			}

			Grid.LoadShared(lua);
		}

		public void Unload(ILua lua) {}
	}
}
