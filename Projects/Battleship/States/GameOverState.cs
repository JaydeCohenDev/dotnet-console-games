using System;

namespace Battleship;

public class GameOverState
{
	private Board playerBoard, enemyBoard;
	
	public GameOverState(GameRenderer renderer, InputHandler inputHandler, Board playerBoard, Board enemyBoard)
	{
		this.playerBoard = playerBoard;
		this.enemyBoard = enemyBoard;
		
		Console.Clear();
		renderer.renderMessage = () =>
		{
			Console.WriteLine();
			switch ((playerBoard.HasWon(), enemyBoard.HasWon()))
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
		renderer.RenderMainView(showEnemyShips: true);
		inputHandler.GetEnterOrEscape();
	}
}