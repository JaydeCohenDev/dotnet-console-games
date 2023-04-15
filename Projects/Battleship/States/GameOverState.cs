using System;

namespace Battleship;

public class GameOverState
{
	public GameOverState()
	{
		Console.Clear();
		Program.renderMessage = () =>
		{
			Console.WriteLine();
			switch ((Program.playerBoard.HasWon(), Program.enemyBoard.HasWon()))
			{
				case (true, true):
					Console.WriteLine("  Draw! All ships were sunk.");
					break;
				case (false, true):
					Console.WriteLine("  You Win! You sunk all the enemy ships.");
					break;
				case (true, false):
					Console.WriteLine("  You Lose! The enemy sunk all your ships.");
					break;
			}

			Console.WriteLine();
			Console.WriteLine("  Play again [enter] or quit [escape]?");
		};
		Program.RenderMainView(showEnemyShips: true);
		Program.inputHandler.GetEnterOrEscape();
	}
}