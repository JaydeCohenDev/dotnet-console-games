using System;
using Battleship;

public static class Program
{
	public static Board playerBoard;
	public static Board enemyBoard;
	public static bool isPlacing;
	public static Placement currentPlacement;
	public static GridPoint gridSelection;
	public static bool isSelecting;
	public static InputHandler inputHandler = new ();
	public static GameRenderer renderer = new ();
	
	public static void Main(string[] args)
	{
		playerBoard = new Board(10, 10);
		enemyBoard = new Board(10, 10);
		
		try
		{
			renderer.SetupConsole();

			while (!inputHandler.hasPressedEscape)
			{
				// introduction screen
				new IntroductionState();
				if (inputHandler.hasPressedEscape) return;

				// ship placement
				new PlayerPlacementState();
				if (inputHandler.hasPressedEscape) return;
				new EnemyPlacementState();

				// shooting phase
				var shootingPhase = new ShootingPhaseState();
				if (shootingPhase.Run()) return;

				// game over
				new GameOverState();
			}
		}
		catch (Exception ex)
		{
			renderer.exception = ex;
			throw;
		}
		finally
		{
			new ShutdownState();
		}
	}
}