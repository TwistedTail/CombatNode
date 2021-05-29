using CombatNode.Mapping;
using System.Collections.Generic;
using System.Numerics;

namespace CombatNode.Paths
{
	public struct PathFinder
	{
		private readonly Dictionary<Node, Node> CameFrom;
		private readonly Dictionary<Node, float> gScore;
		private readonly Dictionary<string, Node> Nodes;
		private readonly PriorityQueue Queue;
		private readonly Stack<Node> Result;
		private readonly Node End;

		public PathFinder(Grid grid, Node start, Node end)
		{
			CameFrom = new();
			gScore = new();
			Nodes = grid.Nodes;
			Queue = new(start);
			Result = new();
			End = end;

			gScore.Add(start, 0f);
		}
		
		private float Heuristic(Node from)
		{
			return (from.FootPos - End.FootPos).Length();
		}

		private Stack<Node> GetResults(Node last)
		{
			Vector3 Direction = Vector3.Zero;
			Node Previous = last;

			Result.Push(last);

			while (CameFrom.TryGetValue(Previous, out Node Current))
			{
				Vector3 Normal = Vector3.Normalize(Previous.FootPos - Current.FootPos);

				if (Direction != Normal)
				{
					Result.Push(Current);

					Direction = Normal;
				}

				Previous = Current;
			}

			return Result;
		}

		public Stack<Node> FindPath()
		{
			while (Queue.Count > 0)
			{
				Node Current = Queue.Dequeue(); // Node with the lowest f score to end

				if (Current.Equals(End))
				{
					return GetResults(Current);
				}

				float BaseCost = gScore[Current];

				foreach (var Entry in Current.Sides)
				{
					Node Side = Nodes[Entry.Key];
					float MoveCost = BaseCost + Entry.Value;
					bool Exists = gScore.ContainsKey(Side);

					if (!Exists || MoveCost < gScore[Side])
					{
						if (Exists)
						{
							gScore.Remove(Side);
							CameFrom.Remove(Side);
						}

						gScore.Add(Side, MoveCost);
						CameFrom.Add(Side, Current);

						Queue.Enqueue(Side, MoveCost + Heuristic(Side));
					}
				}
			}

			return Result; //NOTE: Maybe use GetResults here anyway?
		}
	}
}
