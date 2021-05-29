using CombatNode.Mapping;
using System.Collections.Generic;

namespace CombatNode.Paths
{
	public struct PriorityQueue
	{
		private readonly List<KeyValuePair<Node, float>> Nodes;
		public readonly int Count => Nodes.Count;

		public PriorityQueue(Node initial)
		{
			Nodes = new() {
				new KeyValuePair<Node, float>(initial, 0f)
			};
		}

		public void Enqueue(Node node, float cost)
		{
			Nodes.Add(new KeyValuePair<Node, float>(node, cost));
		}

		// TODO: This class needs to be a binary heap, so we can optimize this turd down here.
		public Node Dequeue()
		{
			int Index = 0;
			var First = Nodes[0];
			Node Result = First.Key;
			float Lowest = First.Value;

			for (int I = 1; I < Nodes.Count; I++)
			{
				var Entry = Nodes[I];

				if (Entry.Value < Lowest)
				{
					Index = I;
					Result = Entry.Key;
					Lowest = Entry.Value;
				}
			}

			Nodes.RemoveAt(Index);

			return Result;
		}
	}
}
