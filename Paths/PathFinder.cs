using CombatNode.Mapping;
using System.Collections.Generic;

namespace CombatNode.Paths
{
	public class PathFinder
	{
		private readonly Dictionary<Node, Node> CameFrom;
		private readonly Dictionary<Node, float> gScore;
		private readonly Dictionary<Node, float> fScore;
		private readonly HashSet<Node> OpenNodes;
		private readonly Stack<Node> Result;
		private Node End;

		public PathFinder()
		{
			CameFrom = new Dictionary<Node, Node>();
			gScore = new Dictionary<Node, float>();
			fScore = new Dictionary<Node, float>();
			OpenNodes = new HashSet<Node>();
			Result = new Stack<Node>();
		}
		
		private float GetCost(Node from)
		{
			return (from.FootPos - End.FootPos).Length();
		}

		private void ClearSide(Node side)
		{
			CameFrom.Remove(side);
			gScore.Remove(side);
			fScore.Remove(side);
		}

		private void UpdateSide(Node side, Node current, float cost)
		{
			CameFrom.Add(side, current);
			gScore.Add(side, cost);
			fScore.Add(side, cost + GetCost(side));

			if (!OpenNodes.Contains(side))
			{
				OpenNodes.Add(side);
			}
		}

		// TODO: Reduce the amount of Nodes by comparing their direction
		private Stack<Node> GetResults(Node last)
		{
			Result.Push(last);
			CameFrom.TryGetValue(last, out Node Previous);

			while (Previous != null)
			{
				Result.Push(Previous);
				CameFrom.TryGetValue(Previous, out Previous);
			}

			return Result;
		}

		public Stack<Node> FindPath(Node start, Node end)
		{
			Node Current = start; // Node with the lowest f score to end

			End = end;

			fScore.Add(start, GetCost(start));
			gScore.Add(start, 0f);
			OpenNodes.Add(start);

			while (OpenNodes.Count > 0)
			{
				if (Current.Equals(end)) {
					return GetResults(Current);
				}

				float BaseCost = gScore[Current];

				OpenNodes.Remove(Current);

				foreach (KeyValuePair<Node, float> Entry in Current.Sides)
				{
					float SideCost = Entry.Value;
					float MoveCost = BaseCost + SideCost;

					if (!gScore.ContainsKey(Entry.Key))
					{
						UpdateSide(Entry.Key, Current, MoveCost);
					}
					else if (MoveCost < gScore[Entry.Key])
					{
						ClearSide(Entry.Key);
						UpdateSide(Entry.Key, Current, MoveCost);
					}
				}

				// NOTE: This is terrible and not optimal
				float Lowest = float.MaxValue;

				foreach (Node open in OpenNodes)
				{
					float Cost = fScore[open];

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
