using System;
using System.Linq;
using System.Collections.Generic;

namespace GoapSharp
{
	public class ListStorage : IStorage
	{
		public List<AStarSharpNode> _opened;
		public List<AStarSharpNode> _closed;

		public ListStorage ()
		{
			_opened = new List<AStarSharpNode> (16);
			_closed = new List<AStarSharpNode> (16);
		}

		public AStarSharpNode FindOpened (AStarSharpNode node)
		{
			for (var i = 0; i < _opened.Count; i++) {
//				long care = node.ws.dontcare ^ -1L;
//				if ((node.ws.values & care) == (_opened[i].ws.values & care)) {
//					return _closed [i];
//				}

				if (node.Equals (_opened [i])) {
					return _opened [i];
				}
			}
			return null;
		}

		public AStarSharpNode FindClosed (AStarSharpNode node)
		{
			for (var i = 0; i < _closed.Count; i++) {
//				long care = node.ws.dontcare ^ -1L;
//				if ((node.ws.values & care) == (_closed[i].ws.values & care)) {
//					return _closed [i];
//				}
				if (node.Equals (_closed [i])) {
					return _closed [i];
				}

			}
			return null;
		}

		public bool HasOpened ()
		{
			return _opened.Count > 0;
		}

		public void RemoveOpened (AStarSharpNode node)
		{
			_opened.Remove (node);
		}

		public void RemoveClosed (AStarSharpNode node)
		{
			_closed.Remove (node);
		}

		public bool IsOpen (AStarSharpNode node)
		{
			return _opened.Contains (node);
		}

		public bool IsClosed (AStarSharpNode node)
		{
			return _closed.Contains (node);
		}

		public void AddToOpenList (AStarSharpNode node)
		{
			_opened.Add (node);
		}

		public void AddToClosedList (AStarSharpNode node)
		{
			_closed.Add (node);
		}

		public AStarSharpNode RemoveCheapestOpenNode ()
		{
			int lowestVal = int.MaxValue;
			int lowestIdx = -1;
			for (int i = 0; i < _opened.Count; i++) {
				if (_opened [i].costSoFarAndHeurisitcCost < lowestVal) {
					lowestVal = _opened [i].costSoFarAndHeurisitcCost;
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

