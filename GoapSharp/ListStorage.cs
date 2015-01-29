using System;
using System.Linq;
using System.Collections.Generic;

namespace GoapSharp
{
	public class ListStorage {
		public List<AStarNode> _opened;
		public List<AStarNode> _closed;

		internal ListStorage() {
			_opened = new List<AStarNode> ();
			_closed = new List<AStarNode> ();
		}

		internal AStarNode FindOpened(AStarNode node) {
			for (var i = 0; i < _opened.Count; i++) {
				if (node.Equals (_opened [i])) {
					return _opened [i];
				}
			}
			return AStarSharp.empty;
		}

		internal AStarNode FindClosed(AStarNode node) {
			for (var i = 0; i < _closed.Count; i++) {
				if (node.Equals (_closed [i])) {
					return _closed [i];
				}
			}
			return AStarSharp.empty;
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
			int lowestVal= int.MaxValue;
			int lowestIdx = -1;
			for ( int i=0; i<_opened.Count; i++ )
			{
				if ( _opened[ i ].costSoFarAndHeurisitcCost < lowestVal )
				{
					lowestVal = _opened[ i ].costSoFarAndHeurisitcCost;
					lowestIdx = i;
				}
			}
			var val = _opened [lowestIdx];
			_opened.RemoveAt (lowestIdx);
			return val;


//			var item = _opened.Min ();
//			_opened.Remove (item);
//			return item;
		}
	}
}

