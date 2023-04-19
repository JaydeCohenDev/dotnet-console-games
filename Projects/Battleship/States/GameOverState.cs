using System;

namespace Battleship.States;

public class GameOverState : IGameState
{
	private readonly InputHandler _inputHandler;
	private readonly Board _playerBoard;
	private readonly Board _enemyBoard;

	public GameOverState(InputHandler inputHandler, Board playerBoard, Board enemyBoard)
	{
		_inputHandler = inputHandler;
		_playerBoard = playerBoard;
		_enemyBoard = enemyBoard;
	}

	public bool Enter()
	{
		Console.Clear();
		GameRenderer.Global.RenderMessage = () =>
		{
			Console.WriteLine();
			switch ((_playerBoard.HasWon(), _enemyBoard.HasWon()))
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
		GameRenderer.Global.RenderMainView(showEnemyShips: true);
		_inputHandler.GetEnterOrEscape();
		return false;
	}

	public void Render(GameRenderer renderer)
	{
		throw new NotImplementedException();
	}
}