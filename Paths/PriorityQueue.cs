using CombatNode.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace CombatNode.Paths
{
	public struct PriorityQueue
	{
		private readonly Dictionary<Node, float> Nodes;
		public readonly int Count => Nodes.Count;

		public PriorityQueue(Node initial)
		{
			Nodes = new() {
				{ initial, 0f }
			};
		}

		public void Enqueue(Node node, float cost)
		{
			Nodes.Remove(node);
			Nodes.Add(node, cost);
		}

		// TODO: This class needs to be a binary heap, so we can optimize this turd down here.
		public Node Dequeue()
		{
			var First = Nodes.First();
			Node Result = First.Key;
			float Lowest = First.Value;

			foreach (var Entry in Nodes)
			{
				if (Entry.Value < Lowest)
				{
					Result = Entry.Key;
					Lowest = Entry.Value;
				}
			}

			Nodes.Remove(Result);

			return Result;
		}
	}
}
