using GmodNET.API;
using System.Numerics;

namespace CombatNode.Mapping
{
	public class Node
	{
		public readonly Grid Parent;
		public readonly Vector3 Coordinates;
		public readonly Vector3 Position;
		public readonly Vector3 FootPos;
		public bool Crouch = false;
		public bool Swim = false;
		public float Depth = 0;

		public Node(Grid grid, Vector3 coords, Vector3 footPos)
		{
			Parent = grid;
			Coordinates = coords;
			Position = coords * grid.NodeSize;
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

			if (Swim)
			{
				lua.PushBool(true);
				lua.SetField(-2, "Swim");
			}
			else
			{
				if (Crouch)
				{
					lua.PushBool(true);
					lua.SetField(-2, "Crouch");
				}
					
				if (Depth > 0)
				{
					lua.PushNumber(Depth);
					lua.SetField(-2, "Depth");
				}
			}
		}
	}
}
