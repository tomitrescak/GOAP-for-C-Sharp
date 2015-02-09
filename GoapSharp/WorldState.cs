using System;
using System.Text;

namespace GoapSharp
{

    public struct WorldState : IEquatable<WorldState>
	{
		public long values;
		public long dontcare;

        public WorldState(long values, long dontcare)
        {
            this.values = values;
            this.dontcare = dontcare;
        }

		#region IEquatable implementation

		public bool Equals (WorldState other)
		{
			long care = dontcare ^ -1L;
			return (values & care) == (other.values & care);
		}

		#endregion

		public bool Set(int idx, bool value )
		{
			this.values = value ? ( this.values | ( 1 << idx ) ) : ( this.values & ~( 1 << idx ));
			this.dontcare ^= (1 << idx);
			return true;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			for ( int i=0; i<ActionPlanner.MAXATOMS; i++ )
			{
				if (( this.dontcare & ( 1L << i ) ) == 0 )
				{
					string val = ActionPlanner.Instance.conditionNames[ i ];
					if (val == null)
						continue;
					string upval = val.ToUpper();
					bool set = ( ( this.values & ( 1L << i ) ) != 0L );
			
					sb.Append (set ? upval : val);
					sb.Append (" | ");
				}
			}
			return sb.ToString ();
		}
	}
}

