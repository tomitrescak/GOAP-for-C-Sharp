using System;

namespace GoapSharp
{
	public interface IStorage
	{
		AStarNode FindOpened(AStarNode node);
		AStarNode FindClosed(AStarNode node);
		bool HasOpened();
		void RemoveOpened(AStarNode node);
		void RemoveClosed(AStarNode node);
		bool IsOpen(AStarNode node);
		bool IsClosed(AStarNode node);
		void AddToOpenList(AStarNode node);
		void AddToClosedList(AStarNode node);
		AStarNode RemoveCheapestOpenNode();
	}
}

