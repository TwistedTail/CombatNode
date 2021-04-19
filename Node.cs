using GmodNET.API;
using System.Collections.Generic;
using System.Numerics;

namespace CombatNode
{
	public class Node
	{
		public readonly Vector3 Coordinates;
		public readonly Vector3 Position;
		public readonly Vector3 FeetPos;
		public readonly Dictionary<Node, float> Sides;
		public bool Crouch = false;
		public bool Swim = false;
		public float Depth = 0;

		public Node(Vector3 coords, Vector3 feetPos)
		{
			Coordinates = coords;
			Position = coords * Grid.NodeSize;
			FeetPos = feetPos;
			Sides = new Dictionary<Node, float>();
		}

		// TODO: Store more information inside of this
		public void Connect(Node target)
		{
			if (target == null) { return; }

			Sides.Remove(target);
			Sides.Add(target, (FeetPos - target.FeetPos).Length());
		}

		public void Disconnect(Node target)
		{
			if (target == null) { return; }

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

		// TODO: Test this thing
		public void PushToLua(ILua lua)
		{
			lua.CreateTable();
			lua.PushVector(Coordinates);
			lua.SetField(-2, "Coordinates");
			lua.PushVector(Position);
			lua.SetField(-2, "Position");

			if (FeetPos.Length() > 0)
			{
				lua.PushVector(FeetPos);
				lua.SetField(-2, "FeetPos");
			}

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
