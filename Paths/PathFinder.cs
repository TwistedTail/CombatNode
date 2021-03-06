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
		private readonly bool UseLocked;
		private readonly Node End;
		
		public PathFinder(Grid grid, Node start, Node end, bool useLocked)
		{
			CameFrom = new();
			gScore = new();
			Nodes = grid.Nodes;
			Queue = new(start);
			Result = new();
			UseLocked = useLocked;
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

		private void CheckSides(Node current)
		{
			float BaseCost = gScore[current];

			foreach (var Entry in current.Sides)
			{
				Node Side = Nodes[Entry.Key];

				if (!UseLocked & !Side.Equals(End) & Side.Locked) { continue; }

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
					CameFrom.Add(Side, current);

					Queue.Enqueue(Side, MoveCost + Heuristic(Side));
				}
			}
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

				CheckSides(Current);
			}

			return Result; //NOTE: Maybe use GetResults here anyway?
		}
	}
}
