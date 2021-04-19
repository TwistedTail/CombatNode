using GmodNET.API;
using System.Collections.Generic;
using System.Numerics;

namespace CombatNode.Mapping
{
	public class Node
	{
		public readonly Vector3 Coordinates;
		public readonly Vector3 Position;
		public readonly Vector3 FootPos;
		public readonly Dictionary<Node, float> Sides;
		public bool Crouch = false;
		public bool Swim = false;
		public float Depth = 0;

		public Node(Vector3 coords, Vector3 footPos)
		{
			Coordinates = coords;
			Position = coords * Grid.NodeSize;
			FootPos = footPos;
			Sides = new Dictionary<Node, float>();
		}

		// TODO: Store more information inside of this
		public void Connect(Node target)
		{
			Sides.Remove(target);
			Sides.Add(target, (FootPos - target.FootPos).Length());
		}

		public void Disconnect(Node target)
		{
			Sides.Remove(target);
		}

		public void Remove()
		{
			foreach (var Node in Sides.Keys)
			{
				Node.Disconnect(this);

				Disconnect(Node); // NOTE: Would this be necessary?
			}
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
