using System;
using System.Linq;
using System.Collections.Generic;

namespace GoapSharp
{
	public class AStarSharp
	{
		class Storage {
			public List<AStarNode> _opened;
			public List<AStarNode> _closed;

			internal Storage() {
				_opened = new List<AStarNode> ();
				_closed = new List<AStarNode> ();
			}

			internal AStarNode? FindOpened(AStarNode node) {
				for (var i = 0; i < _opened.Count; i++) {
					if (node.Equals (_opened [i])) {
						return _opened [i];
					}
				}
				return null;
			}

			internal AStarNode? FindClosed(AStarNode node) {
				for (var i = 0; i < _closed.Count; i++) {
					if (node.Equals (_closed [i])) {
						return _closed [i];
					}
				}
				return null;
			}

			internal bool HasOpened() {
				return _opened.Count > 0;
			}

			internal void RemoveOpened(AStarNode node) {
				_opened.Remove (node);
			}

			internal void RemoveClosed(AStarNode node) {
				_closed.Remove (node);
			}
				
			internal bool IsOpen(AStarNode node) {
				return _opened.Contains (node);
			}

			internal bool IsClosed(AStarNode node) {
				return _closed.Contains (node);
			}

			internal void AddToOpenList(AStarNode node) {
				_opened.Add (node);
			}

			internal void AddToClosedList(AStarNode node) {
				_closed.Add (node);
			}

			internal AStarNode RemoveCheapestOpenNode() {
				var item = _opened.Min ();
				_opened.Remove (item);
				return item;
			}
		}

		public static List<AStarNode> Plan (ActionPlanner ap, WorldState start, WorldState goal)
		{
			var storage = new Storage ();

			AStarNode currentNode;
			currentNode.ws = start;
			currentNode.parentws = start;
			currentNode.costSoFar = 0; // g
			currentNode.heuristicCost = CalculateHeuristic( start, goal ); // h
			currentNode.costSoFarAndHeurisitcCost = currentNode.costSoFar + currentNode.heuristicCost; // f
			currentNode.actionname = null;
			currentNode.parent = new WeakReference (null);

			storage.AddToOpenList (currentNode);

			while (true) {
				if (!storage.HasOpened()) { // Console.WriteLine( "Did not find a path." ); 
					return null; 
				}

				currentNode = storage.RemoveCheapestOpenNode ();
				// Console.WriteLine ("--------------------------------------\n");
				// Console.WriteLine("CurrentNode: " + currentNode);
				// Console.WriteLine("CurrentState: " + currentNode.ws);
				// Console.WriteLine(string.Format("Opened: {0}    Closed: {1}", storage._opened.Count, storage._closed.Count));

				storage.AddToClosedList (currentNode);
				// Console.WriteLine("CLOSING: " + currentNode);

				if (goal.Equals (currentNode.ws)) {
					// Console.WriteLine ("Finished with plan");
					return ReconstructPlan (currentNode);
				}

				var neighbours = ap.GetPossibleTransitions1 (currentNode.ws);

				for (var i = 0; i < neighbours.Count; i++) {
					var cur = neighbours [i];

					// Console.WriteLine("Processing {0} -> {1}", currentNode.actionname, cur.actionname);
					// Console.WriteLine("State: " + cur.ws);

					var opened = storage.FindOpened (cur);
					var closed = storage.FindClosed (cur);
					int cost = currentNode.costSoFar + cur.costSoFar;

					// Console.WriteLine("Cost: {0}  Idx Opened: {1}  Idx Closed: {2}", cost, opened, closed);

					// if neighbor in OPEN and cost less than g(neighbor):
					if ( opened.HasValue && cost < opened.Value.costSoFar )
					{
						// Console.WriteLine("OPENED Neighbor: " + opened.Value.ws);
						// Console.WriteLine("neighbor in OPEN and cost less than g(neighbor)");

						// remove neighbor from OPEN, because new path is better
						storage.RemoveOpened (opened.Value);
						opened = null;
					}

					// if neighbor in CLOSED and cost less than g(neighbor):
					if ( closed.HasValue && cost < closed.Value.costSoFar )
					{
						// Console.WriteLine("CLOSED Neighbor: " + closed.Value.ws);
						// Console.WriteLine("neighbor in CLOSED and cost less than g(neighbor)");

						// remove neighbor from CLOSED
						storage.RemoveClosed (closed.Value);
					}

					// if neighbor not in OPEN and neighbor not in CLOSED:
					if (!opened.HasValue && !closed.HasValue)
					{
						AStarNode nb;
						nb.ws = cur.ws;
						nb.costSoFar = cost;
						nb.heuristicCost = CalculateHeuristic( cur.ws, goal );
						nb.costSoFarAndHeurisitcCost = nb.costSoFar + nb.heuristicCost;
						nb.actionname = cur.actionname;
						nb.parentws = currentNode.ws;
						nb.parent = new WeakReference (currentNode);
						storage.AddToOpenList (nb);

						// Console.WriteLine("NEW OPENED: " + nb.ToString());
					}
					// Console.WriteLine("\n--\n");
				}
			}
		}

		//!< Internal function to reconstruct the plan by tracing from last node to initial node.
		static List<AStarNode> ReconstructPlan(AStarNode goalnode)
		{	
			var plan = new List<AStarNode> ();

			AStarNode curnode = goalnode;
			while (!string.IsNullOrEmpty(curnode.actionname))
			{

				plan.Add (curnode);
				if (curnode.parent.Target != null) {
					curnode = (AStarNode)curnode.parent.Target;
				} else {
					break;
				}
			}
			return plan;
		}



		//!< This is our heuristic: estimate for remaining distance is the nr of mismatched atoms that matter.
		static int CalculateHeuristic( WorldState fr, WorldState to )
		{
			long care = ( to.dontcare ^ -1L );
			long diff =  ( fr.values & care ) ^ ( to.values & care ) ;
			int dist=0;
			for ( int i=0; i< ActionPlanner.MAXATOMS; ++i )
				if ( ( diff & ( 1L << i ) ) != 0 ) dist++;
			return dist;
		}
	}
}

