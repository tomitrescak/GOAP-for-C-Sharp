using System;

namespace GoapSharp
{
	public interface IStorage
	{
		AStarSharpNode FindOpened(AStarSharpNode node);
		AStarSharpNode FindClosed(AStarSharpNode node);
		bool HasOpened();
		void RemoveOpened(AStarSharpNode node);
		void RemoveClosed(AStarSharpNode node);
		bool IsOpen(AStarSharpNode node);
		bool IsClosed(AStarSharpNode node);
		void AddToOpenList(AStarSharpNode node);
		void AddToClosedList(AStarSharpNode node);
		AStarSharpNode RemoveCheapestOpenNode();
	}
}

