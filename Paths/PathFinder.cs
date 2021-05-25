using CombatNode.Mapping;
using System.Collections.Generic;
using System.Numerics;

namespace CombatNode.Paths
{
	public class PathFinder
	{
		private readonly Dictionary<Node, Node> CameFrom;
		private readonly Dictionary<Node, int> gScore;
		private readonly Dictionary<Node, int> fScore;
		private readonly HashSet<Node> OpenNodes;
		private readonly Stack<Node> Result;
		private readonly Grid Map;
		private Node End;

		public PathFinder(Grid grid)
		{
			CameFrom = new();
			gScore = new();
			fScore = new();
			OpenNodes = new();
			Result = new();
			Map = grid;
		}
		
		private int GetCost(Node from)
		{
			return (int)(from.FootPos - End.FootPos).Length();
		}

		private void ClearSide(Node side)
		{
			CameFrom.Remove(side);
			gScore.Remove(side);
			fScore.Remove(side);
		}

		private void UpdateSide(Node side, Node current, int cost)
		{
			CameFrom.Add(side, current);
			gScore.Add(side, cost);
			fScore.Add(side, cost + GetCost(side));

			if (!OpenNodes.Contains(side))
			{
				OpenNodes.Add(side);
			}
		}

		private Stack<Node> GetResults(Node last)
		{
			Vector3 Direction = new();
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

		public Stack<Node> FindPath(Node start, Node end)
		{
			Node Current = start; // Node with the lowest f score to end

			End = end;

			fScore.Add(start, GetCost(start));
			gScore.Add(start, 0);
			OpenNodes.Add(start);

			while (OpenNodes.Count > 0)
			{
				if (Current.Equals(end)) {
					return GetResults(Current);
				}

				OpenNodes.Remove(Current);

				int BaseCost = gScore[Current];

				foreach (KeyValuePair<string, ushort> Entry in Current.Sides)
				{
					if (!Map.Nodes.TryGetValue(Entry.Key, out Node Side)) { continue; }

					int SideCost = Entry.Value;
					int MoveCost = BaseCost + SideCost;

					if (!gScore.ContainsKey(Side))
					{
						UpdateSide(Side, Current, MoveCost);
					}
					else if (MoveCost < gScore[Side])
					{
						ClearSide(Side);
						UpdateSide(Side, Current, MoveCost);
					}
				}

				// NOTE: This is terrible and not optimal
				int Lowest = int.MaxValue;

				foreach (Node open in OpenNodes)
				{
					int Cost = fScore[open];

					if (Cost < Lowest)
					{
						Current = open;
						Lowest = Cost;
					}
				}
			}

			return Result; //NOTE: Maybe use GetResults here anyway?
		}
	}
}
