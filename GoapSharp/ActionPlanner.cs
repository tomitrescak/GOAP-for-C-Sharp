using System;
using System.Text;
using System.Collections.Generic;

namespace GoapSharp
{
	public class ActionPlanner
	{
		public const int MAXATOMS = 64;
		public const int MAXACTIONS = 64;

		public string[] conditionNames = new string[MAXACTIONS];
		//!< Names associated with all world state atoms.
		public string[] actionNames = new string[MAXACTIONS];
		//!< Names of all actions in repertoire.
		WorldState[] preConditions = new WorldState[MAXACTIONS];
		//!< Preconditions for all actions.
		WorldState[] postConditions = new WorldState[MAXACTIONS];
		//!< Postconditions for all actions (action effects).
		int[] act_costs = new int[MAXACTIONS];
		//!< Cost for all actions.

		int numatoms;
		//!< Number of world state atoms.
		int numactions;
		//!< The number of actions in out repertoire.

		private static ActionPlanner _instance;
		public static ActionPlanner Instance 
		{
			get {
				if (_instance == null) {
					_instance = new ActionPlanner ();
				}
				return _instance;
			}
		}

		public ActionPlanner ()
		{
			this.numatoms = 0;
			this.numactions = 0;
			for (int i = 0; i < MAXATOMS; ++i) {
				this.conditionNames [i] = null;
				this.actionNames [i] = null;
				this.act_costs [i] = 0;

				this.preConditions [i] = new WorldState (0, -1);
				this.postConditions [i] = new WorldState (0, -1);
			}
		}

		public bool SetPrecondition (string actionname, string atomname, bool value)
		{
			int actidx = FindActionNameIndex (actionname);
			int atmidx = FindConditionNameIndex (atomname);
			if (actidx == -1 || atmidx == -1)
				return false;

			this.preConditions [actidx].Set (atmidx, value);
			return true;
		}

		public bool SetPostcondition (string actionname, string atomname, bool value)
		{
			int actidx = FindActionNameIndex (actionname);
			int atmidx = FindConditionNameIndex (atomname);
			if (actidx == -1 || atmidx == -1)
				return false;
			this.postConditions [actidx].Set (atmidx, value);
			return true;
		}


		public bool SetCost (string actionname, int cost)
		{
			int actidx = FindActionNameIndex (actionname);
			if (actidx == -1)
				return false;
			this.act_costs [actidx] = cost;
			return true;
		}

		public string Describe ()
		{
			var sb = new StringBuilder ();

			for (int a = 0; a < this.numactions; ++a) {
				sb.AppendLine(this.actionNames[a]);

				WorldState pre = this.preConditions [a];
				WorldState pst = this.postConditions [a];
				for (int i = 0; i < MAXATOMS; ++i) {
					if ((pre.dontcare & (1L << i)) == 0) {
						bool v = (pre.values & (1L << i)) != 0;
						sb.AppendFormat ("  {0}=={1}\n", this.conditionNames [i], v);
					}
				}
				for (int i = 0; i < MAXATOMS; ++i) {
					if ((pst.dontcare & (1L << i)) == 0) {
						bool v = (pst.values & (1L << i)) != 0;
						sb.AppendFormat ("  {0}:={1}\n", this.conditionNames [i], v);
					}
				}
			}

			return sb.ToString ();
		}

		// static


		public int FindConditionNameIndex (string atomname)
		{
			int idx;
			for (idx = 0; idx < this.numatoms; ++idx) {
				if (string.Equals (this.conditionNames [idx], atomname))
					return idx;
			}

			if (idx < MAXATOMS - 1) {
				this.conditionNames [idx] = atomname;
				this.numatoms++;
				return idx;
			}

			return -1;
		}

		public int FindActionNameIndex (string actionname)
		{
			int idx;
			for (idx = 0; idx < this.numactions; idx++) {
				if (string.Equals (this.actionNames [idx], actionname))
					return idx;
			}

			if (idx < MAXACTIONS - 1) {
				this.actionNames [idx] = actionname;
				this.act_costs [idx] = 1; // default cost is 1
				this.numactions++;
				return idx;
			}

			return -1;
		}

		public List<AStarSharpNode> GetPossibleTransitions1(WorldState fr)
		{
			var result = new List<AStarSharpNode> (10);
			for ( int i=0; i<this.numactions; ++i )
			{
				// see if precondition is met
				WorldState pre = this.preConditions[ i ];
				long care = ( pre.dontcare ^ -1L );
				bool met = ( ( pre.values & care ) == ( fr.values & care ) );
				if ( met )
				{
					AStarSharpNode node = new AStarSharpNode();
					node.actionname = this.actionNames [i];
					node.costSoFar = this.act_costs [i];
					node.ws = ApplyPostConditions(this, i, fr );
					result.Add (node);
				}
			}
			return result;
		}

		public int GetPossibleTransitions(WorldState fr, WorldState[] to, string[] actionnames, int[] actioncosts, int cnt )
		{
			int writer=0;
			for ( int i=0; i<this.numactions && writer<cnt; ++i )
			{
				// see if precondition is met
				WorldState pre = this.preConditions[ i ];
				long care = ( pre.dontcare ^ -1L );
				bool met = ( ( pre.values & care ) == ( fr.values & care ) );
				if ( met )
				{
					actionnames[ writer ] = this.actionNames[ i ];
					actioncosts[ writer ] = this.act_costs[ i ];
					to[ writer ] = ApplyPostConditions(this, i, fr );
					++writer;
				}
			}
			return writer;
		}

		static WorldState ApplyPostConditions( ActionPlanner ap, int actionnr, WorldState fr )
		{
			WorldState pst = ap.postConditions[ actionnr ];
			long unaffected = pst.dontcare;
			long affected   = ( unaffected ^ -1L );

			fr.values = ( fr.values & unaffected ) | ( pst.values & affected );
			fr.dontcare &= pst.dontcare;
			return fr;

		}

	}
}

