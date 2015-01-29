using System;

namespace GoapSharp
{
	public struct AStarNode : IComparable<AStarNode>, IEquatable<AStarNode> {
		public WorldState ws;		//!< The state of the world at this node.
		public int costSoFar;				//!< The cost so far.
		public int heuristicCost;				//!< The heuristic for remaining cost (don't overestimate!)
		public int costSoFarAndHeurisitcCost;				//!< g+h combined.
		public string actionname;		//!< How did we get to this node?
		public WorldState parentws;		//!< Where did we come from?
		//public WeakReference parent;
		public int depth;

		public override string ToString ()
		{
			return string.Format ("[{0} | {1}]: {2}", costSoFar, heuristicCost, actionname);
		}

		#region IEquatable implementation

		public bool Equals (AStarNode other)
		{
			return ws.Equals(other.ws);
		}

		#endregion

		#region IComparable implementation
		public int CompareTo (AStarNode other)
		{
			return this.costSoFarAndHeurisitcCost.CompareTo(other.costSoFarAndHeurisitcCost);
		}
		#endregion
	}

	public class AStar
	{
		const int MAXOPEN = 1024;	//!< The maximum number of nodes we can store in the opened set.
		const int MAXCLOS = 1024;	//!< The maximum number of nodes we can store in the closed set.

		static AStarNode[] opened = new AStarNode[ MAXOPEN ];	//!< The set of nodes we should consider.
		static AStarNode[] closed = new AStarNode[ MAXCLOS ];	//!< The set of nodes we already visited.

		static int numOpened = 0;	//!< The nr of nodes in our opened set.
		static int numClosed = 0;	//!< The nr of nodes in our closed set.

		public static int Plan (
			ActionPlanner ap,
			WorldState start,
			WorldState goal,
			string[] plan,
			WorldState[] worldstates,
			ref int plansize
		)
		{
			// put start in opened list
			numOpened=0;
			AStarNode n0;
			n0.ws = start;
			n0.parentws = start;
			n0.costSoFar = 0;
			n0.heuristicCost = calc_h( start, goal );
			n0.costSoFarAndHeurisitcCost = n0.costSoFar + n0.heuristicCost;
			n0.actionname = null;
			//n0.parent = new WeakReference (null);
			n0.depth = 1;
			opened[ numOpened++ ] = n0;
			// empty closed list
			numClosed=0;

			do
			{
				if ( numOpened == 0 ) { // Console.WriteLine( "Did not find a path." ); 
					return -1; 
				}
				// find the node with lowest rank
				int lowestIdx=-1;
				int lowestVal= int.MaxValue;
				for ( int i=0; i<numOpened; i++ )
				{
					if ( opened[ i ].costSoFarAndHeurisitcCost < lowestVal )
					{
						lowestVal = opened[ i ].costSoFarAndHeurisitcCost;
						lowestIdx = i;
					}
				}
				// remove the node with the lowest rank
				AStarNode cur = opened[ lowestIdx ];
				if ( numOpened > 0 ) opened[ lowestIdx ] = opened[ numOpened-1 ];
				numOpened--;

				// Console.WriteLine ("--------------------------------------\n");
				// Console.WriteLine("CurrentNode: " + cur.ToString());
				// Console.WriteLine("CurrentState: " + cur.ws);

				// Console.WriteLine(string.Format("Opened: {0}    Closed: {1}", numOpened, numClosed));

				// if it matches the goal, we are done!
				long care = ( goal.dontcare ^ -1L );
				bool match = ( ( cur.ws.values & care ) == ( goal.values & care ) );
				if ( match ) 
				{
					// Console.WriteLine("Matched goal!");
					reconstruct_plan( ap, cur, plan, worldstates, ref plansize );
					return cur.costSoFarAndHeurisitcCost;
				}
				// add it to closed
				closed[ numClosed++ ] = cur;

				// Console.WriteLine("CLOSING: " + cur);

				if ( numClosed == MAXCLOS ) { 
					// Console.WriteLine("Closed set overflow"); 
					return -1; 
				} // ran out of storage for closed set

				// iterate over neighbours
				var actionnames = new string[ActionPlanner.MAXACTIONS ];
				var actioncosts = new int[ ActionPlanner.MAXACTIONS ];
				var to = new WorldState[ ActionPlanner.MAXACTIONS ];
				int numtransitions = ap.GetPossibleTransitions(cur.ws, to, actionnames, actioncosts, ActionPlanner.MAXACTIONS );

				// Console.WriteLine( "Neighbours: " + numtransitions );
				for ( int i=0; i<numtransitions; i++ )
				{
					// Console.WriteLine("Processing {0} -> {1}", cur.actionname, actionnames[i]);
					// Console.WriteLine("State: " + to[i]);

					AStarNode nb;
					int cost = cur.costSoFar + actioncosts[ i ];
					int idx_o = idx_in_opened( to[ i ] );
					int idx_c = idx_in_closed( to[ i ] );
				
					// Console.WriteLine("Cost: {0}  Idx Opened: {1}  Idx Closed: {2}", cost, idx_o, idx_c);

					// if neighbor in OPEN and cost less than g(neighbor):
					if ( idx_o >= 0 && cost < opened[ idx_o ].costSoFar )
					{
						// Console.WriteLine("OPENED Neighbor: " + opened[ idx_o ].ws);
						// Console.WriteLine("neighbor in OPEN and cost less than g(neighbor)");
						// remove neighbor from OPEN, because new path is better
						if ( numOpened > 0 ) { 
							opened[ idx_o ] = opened[ numOpened-1 ];
							numOpened--;						
							// Console.WriteLine("Remove neighbor from OPEN, because new path is better");
						}

						idx_o = -1; // BUGFIX: neighbor is no longer in OPEN, signal this so that we can re-add it.
					}
					// if neighbor in CLOSED and cost less than g(neighbor):
					if ( idx_c >= 0 && cost < closed[ idx_c ].costSoFar )
					{
						// Console.WriteLine("CLOSED Neighbor: " + closed[ idx_c ].ws);
						// Console.WriteLine("neighbor in CLOSED and cost less than g(neighbor)");
						// remove neighbor from CLOSED
						if ( numClosed > 0 ) {
							// Console.WriteLine("remove neighbor from CLOSED");
							closed[ idx_c ] = closed[ numClosed-1 ];
						}
						numClosed--;
					}
					// if neighbor not in OPEN and neighbor not in CLOSED:
					if ( idx_c == -1 && idx_o == -1 )
					{
						nb.ws = to[ i ];
						nb.costSoFar = cost;
						nb.heuristicCost = calc_h( nb.ws, goal );
						nb.costSoFarAndHeurisitcCost = nb.costSoFar + nb.heuristicCost;
						nb.actionname = actionnames[ i ];
						nb.parentws = cur.ws;
						//nb.parent = new WeakReference(cur);
						nb.depth = cur.depth + 1;
						opened[ numOpened++ ] = nb;

						// Console.WriteLine("NEW OPENED: " + nb.ToString());
					}
					if ( numOpened == MAXOPEN ) { 
							// Console.WriteLine("Opened set overflow"); 
					return -1; 
						} // ran out of storage for opened set
					// Console.WriteLine("\n--\n");
				}
			} while( true );

			return -1;
		}


		//!< This is our heuristic: estimate for remaining distance is the nr of mismatched atoms that matter.
		static int calc_h( WorldState fr, WorldState to )
		{
			long care = ( to.dontcare ^ -1L );
			long diff =  ( fr.values & care ) ^ ( to.values & care ) ;
			int dist=0;
			for ( int i=0; i< ActionPlanner.MAXATOMS; ++i )
				if ( ( diff & ( 1L << i ) ) != 0 ) dist++;
			return dist;
		}


		//!< Internal function to look up a world state in our opened set.
		static int idx_in_opened( WorldState ws )
		{
			for ( int i=0; i<numOpened; ++i )
				if ( opened[ i ].ws.values == ws.values ) return i;
			return -1;
		}


		//!< Internal function to lookup a world state in our closed set.
		static int idx_in_closed( WorldState ws )
		{
			for ( int i=0; i<numClosed; ++i )
				if ( closed[ i ].ws.values == ws.values ) return i;
			return -1;
		}


		//!< Internal function to reconstruct the plan by tracing from last node to initial node.
		static void reconstruct_plan(ActionPlanner ap, AStarNode goalnode, string[] plan, WorldState[] worldstates, ref int plansize )
		{
			AStarNode curnode = goalnode;
			int idx = plansize - 1;
			int numsteps=0;
			while (!string.IsNullOrEmpty(curnode.actionname))
			{
				if ( idx >= 0 )
				{
					plan[ idx ] = curnode.actionname;
					worldstates[ idx ] = curnode.ws;
					int i = idx_in_closed( curnode.parentws );
					if (i == -1) {
						--idx;
						numsteps++;
						break;
					}
					curnode = closed[i];
				}
				--idx;
				numsteps++;
			}
			idx++;	// point to last filled

			if ( idx > 0 )
				for ( int i=0; i<numsteps; ++i )
				{
					plan[ i ] = plan[ i + idx ];
					worldstates[ i ] = worldstates[ i + idx ];
				}
			if ( idx < 0 )
				// Console.WriteLine(string.Format("ERROR: Plan of size {0} cannot be returned in buffer of size {1}", numsteps, plansize ));

			plansize = numsteps;
		}
	}
}

