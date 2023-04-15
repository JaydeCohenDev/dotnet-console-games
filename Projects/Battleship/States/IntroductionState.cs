using System;

namespace Battleship;

public class IntroductionState
{
	public IntroductionState()
	{
		Console.Clear();
		Program.renderMessage = () =>
		{
			Console.WriteLine();
			Console.WriteLine("  This is a guessing game where you will place your battle ships");
			Console.WriteLine("  on a grid, and then shoot locations of the enemy grid trying");
			Console.WriteLine("  to find and sink all of their ships.The first player to sink");
			Console.WriteLine("  all the enemy ships wins.");
			Console.WriteLine();
			Console.WriteLine("  Press [escape] at any time to close the game.");
			Console.WriteLine();
			Console.WriteLine("  Press [enter] to begin...");
		};
		Program.RenderMainView();
		Program.GetEnterOrEscape();
	}
}