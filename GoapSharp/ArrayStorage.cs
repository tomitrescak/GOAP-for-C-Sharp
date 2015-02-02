using System;
using System.Linq;
using System.Collections.Generic;

namespace GoapSharp
{
	public class ArrayStorage : IStorage  {
		AStarSharpNode[] _opened;
		AStarSharpNode[] _closed;
		int _numOpened;
		int _numClosed;

		int _lastFoundOpened;
		int _lastFoundClosed;

		internal ArrayStorage() {
			_opened = new AStarSharpNode[64];
			_closed = new AStarSharpNode[64];
		}

		public AStarSharpNode FindOpened(AStarSharpNode node) {
			for (var i = 0; i < _numOpened; i++) {
				long care = node.ws.dontcare ^ -1L;
				if ((node.ws.values & care) == (_opened[i].ws.values & care)) {
					_lastFoundClosed = i;
					return _closed [i];
				}
			}
			return null;
		}

		public AStarSharpNode FindClosed(AStarSharpNode node) {
			for (var i = 0; i < _numClosed; i++) {
				long care = node.ws.dontcare ^ -1L;
				if ((node.ws.values & care) == (_closed[i].ws.values & care)) {
					_lastFoundClosed = i;
					return _closed [i];
				}
			}
			return null;
		}

		public bool HasOpened() {
			return _opened.Length > 0;
		}

		public void RemoveOpened(AStarSharpNode node) {
			if (_numOpened > 0) {
				_opened [_lastFoundOpened] = _opened [_numOpened - 1];
			}
			_numOpened--;
		}

		public void RemoveClosed(AStarSharpNode node) {
			if (_numClosed > 0) {
				_closed [_lastFoundClosed] = _closed [_numClosed - 1];
			}
			_numClosed--;
		}

		public bool IsOpen(AStarSharpNode node) {
			return _opened.Contains (node);
		}

		public bool IsClosed(AStarSharpNode node) {
			return _closed.Contains (node);
		}

		public void AddToOpenList(AStarSharpNode node) {
			_opened[_numOpened++] = (node);
		}

		public void AddToClosedList(AStarSharpNode node) {
			_closed[_numClosed++] = (node);
		}

		public AStarSharpNode RemoveCheapestOpenNode() {
			int lowestVal= int.MaxValue;
			_lastFoundOpened = -1;
			for ( int i=0; i<_numOpened; i++ )
			{
				if ( _opened[ i ].costSoFarAndHeurisitcCost < lowestVal )
				{
					lowestVal = _opened[ i ].costSoFarAndHeurisitcCost;
					_lastFoundOpened = i;
				}
			}
			var val = _opened [_lastFoundOpened];
			RemoveOpened (val);
			return val;
//			var item = _opened.Min ();
//			_opened.Remove (item);
//			return item;
		}
	}
}

