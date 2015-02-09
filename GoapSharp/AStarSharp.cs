using System;
using System.Linq;

namespace GoapSharp
{
	public class AStarSharpNode : IComparable<AStarSharpNode>, IEquatable<AStarSharpNode> {
		public WorldState ws = new WorldState(0, -1);		//!< The state of the world at this node.
		public int costSoFar;				//!< The cost so far.
		public int heuristicCost;				//!< The heuristic for remaining cost (don't overestimate!)
		public int costSoFarAndHeurisitcCost;				//!< g+h combined.
		public string actionname;		//!< How did we get to this node?
		public WorldState parentws = new WorldState(0, -1);		//!< Where did we come from?
		public AStarSharpNode parent;
		public int depth;
	

		public override string ToString ()
		{
			return string.Format ("[{0} | {1}]: {2}", costSoFar, heuristicCost, actionname);
		}

		#region IEquatable implementation

		public bool Equals (AStarSharpNode other)
		{
			//return ws.Equals (other.ws);
			long care = ws.dontcare ^ -1L;
			return (ws.values & care) == (other.ws.values & care);
		}

		#endregion

		#region IComparable implementation
		public int CompareTo (AStarSharpNode other)
		{
			return this.costSoFarAndHeurisitcCost.CompareTo(other.costSoFarAndHeurisitcCost);
		}
		#endregion
	}

	public class AStarSharp
	{
		public static AStarNode empty;

		public static AStarSharpNode[] Plan (ActionPlanner ap, WorldState start, WorldState goal, IStorage storage)
		{
			AStarSharpNode currentNode = new AStarSharpNode();
			currentNode.ws = start;
			currentNode.parentws = start;
			currentNode.costSoFar = 0; // g
			currentNode.heuristicCost = CalculateHeuristic( start, goal ); // h
			currentNode.costSoFarAndHeurisitcCost = currentNode.costSoFar + currentNode.heuristicCost; // f
			currentNode.actionname = null;
			currentNode.parent = null;
			currentNode.depth = 1;

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
					return null;
				}

//				var actionnames = new string[ActionPlanner.MAXACTIONS ];
//				var actioncosts = new int[ ActionPlanner.MAXACTIONS ];
//				var to = new WorldState[ ActionPlanner.MAXACTIONS ];
//				int numtransitions = ap.GetPossibleTransitions(currentNode.ws, to, actionnames, actioncosts, ActionPlanner.MAXACTIONS );

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
					if ( opened != null && cost < opened.costSoFar )
					{
						// Console.WriteLine("OPENED Neighbor: " + opened.Value.ws);
						// Console.WriteLine("neighbor in OPEN and cost less than g(neighbor)");

						// remove neighbor from OPEN, because new path is better
						storage.RemoveOpened (opened);
						opened = null;
					}

					// if neighbor in CLOSED and cost less than g(neighbor):
					if ( closed != null && cost < closed.costSoFar )
					{
						// Console.WriteLine("CLOSED Neighbor: " + closed.Value.ws);
						// Console.WriteLine("neighbor in CLOSED and cost less than g(neighbor)");

						// remove neighbor from CLOSED
						storage.RemoveClosed (closed);
					}

					// if neighbor not in OPEN and neighbor not in CLOSED:
					if (opened == null && closed == null)
					{
						AStarSharpNode nb = new AStarSharpNode();
						nb.ws = cur.ws;
						nb.costSoFar = cost;
						nb.heuristicCost = CalculateHeuristic( cur.ws, goal );
						nb.costSoFarAndHeurisitcCost = nb.costSoFar + nb.heuristicCost;
						nb.actionname = cur.actionname;
						nb.parentws = currentNode.ws;
						nb.parent = currentNode;
						nb.depth = currentNode.depth + 1;
						storage.AddToOpenList (nb);

						// Console.WriteLine("NEW OPENED: " + nb.ToString());
					}
					// Console.WriteLine("\n--\n");
				}
			}
		}

		//!< Internal function to reconstruct the plan by tracing from last node to initial node.
		static AStarSharpNode[] ReconstructPlan(AStarSharpNode goalnode)
		{	
			var plan = new AStarSharpNode[goalnode.depth - 1];

			AStarSharpNode curnode = goalnode;
			for (var i = 0; i < goalnode.depth - 1; i++) {
				plan [i] = curnode;
				curnode = curnode.parent;
			}
//			while (!string.IsNullOrEmpty(curnode.actionname))
//			{
//
//				plan.Add (curnode);
//				if (curnode.parent.Target != null) {
//					curnode = (AStarNode)curnode.parent.Target;
//				} else {
//					break;
//				}
//			}
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

