using CombatNode.Mapping;
using CombatNode.Paths;
using GmodNET.API;

namespace CombatNode
{
	public class Module : IModule
	{
		public string ModuleName => "Combat Node";
		public string ModuleVersion => "0.3";

		public void Load(ILua lua, bool is_serverside, ModuleAssemblyLoadContext assembly_context)
		{
			// Creating global table
			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.CreateTable();
			lua.SetField(-2, "CNode");
			lua.Pop();

			if (is_serverside)
			{
				PathManager.Load(lua);
				GridManager.Load(lua);
			}
		}

		public void Unload(ILua lua) {}
	}
}
