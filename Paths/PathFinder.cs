using CombatNode.Mapping;
using System.Collections.Generic;
using System.Numerics;

namespace CombatNode.Paths
{
	public class PathFinder
	{
		private readonly Dictionary<Node, Node> CameFrom;
		private readonly Dictionary<Node, float> gScore;
		private readonly Dictionary<Node, float> fScore;
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
			Dictionary<string, Sides> Connections = Map.Connections;

			End = end;

			fScore.Add(start, GetCost(start));
			gScore.Add(start, 0f);
			OpenNodes.Add(start);

			while (OpenNodes.Count > 0)
			{
				if (Current.Equals(end)) {
					return GetResults(Current);
				}

				OpenNodes.Remove(Current);

				float BaseCost = gScore[Current];

				if (!Connections.TryGetValue(Current.Coordinates.ToString(), out Sides Result)) { continue; }

				foreach (KeyValuePair<string, float> Entry in Result.Connections)
				{
					if (!Map.Nodes.TryGetValue(Entry.Key, out Node Side)) { continue; }

					float SideCost = Entry.Value;
					float MoveCost = BaseCost + SideCost;

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
