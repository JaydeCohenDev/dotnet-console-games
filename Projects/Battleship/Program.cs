using System;
using Battleship;

public class Program
{
	private static Board playerBoard;
	private static Board enemyBoard;
	public static InputHandler inputHandler;
	public static GameRenderer renderer;
	
	public static void Main(string[] args)
	{
		playerBoard = new Board(10, 10);
		enemyBoard = new Board(10, 10);

		renderer = new GameRenderer(playerBoard, enemyBoard);
		inputHandler = new InputHandler();
		
		try
		{
			renderer.SetupConsole();

			while (!inputHandler.HasPressedEscape)
			{
				// introduction screen
				new IntroductionState(renderer);
				if (inputHandler.HasPressedEscape) return;

				// ship placement
				new PlayerPlacementState(renderer, inputHandler, playerBoard);
				if (inputHandler.HasPressedEscape) return;
				new EnemyPlacementState(renderer, enemyBoard);

				// shooting phase
				var shootingPhase = new ShootingPhaseState(renderer, inputHandler, playerBoard, enemyBoard);
				if (shootingPhase.Run()) return;

				// game over
				new GameOverState(renderer, inputHandler, playerBoard, enemyBoard);
			}
		}
		catch (Exception ex)
		{
			renderer.exception = ex;
			throw;
		}
		finally
		{
			new ShutdownState(renderer);
		}
	}
}