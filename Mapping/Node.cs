using GmodNET.API;
using Newtonsoft.Json;
using System.Numerics;

namespace CombatNode.Mapping
{
	public class Node
	{
		[JsonProperty]
		public readonly Vector3 Coordinates;
		[JsonProperty]
		public readonly Vector3 Position;
		[JsonProperty]
		public readonly Vector3 FootPos;

		[JsonConstructor]
		public Node(Vector3 size, Vector3 coords, Vector3 footPos)
		{
			Coordinates = coords;
			Position = coords * size;
			FootPos = footPos;
		}

		public void PushToLua(ILua lua)
		{
			lua.CreateTable();
			lua.PushVector(Coordinates);
			lua.SetField(-2, "Coordinates");
			lua.PushVector(Position);
			lua.SetField(-2, "Position");
			lua.PushVector(FootPos);
			lua.SetField(-2, "FootPos");
		}
	}
}
