using System;
using Battleship.States;

namespace Battleship;

public class Program
{
	private readonly Board playerBoard;
	private readonly Board enemyBoard;
	private readonly InputHandler inputHandler;
	private readonly GameRenderer renderer;
	
	public static void Main(string[] args)
	{
		var program = new Program();
		program.Start();
	}

	public Program()
	{
		playerBoard = new Board(10, 10);
		enemyBoard = new Board(10, 10);

		renderer = new GameRenderer(playerBoard, enemyBoard);
		inputHandler = new InputHandler();
	}

	public void Start()
	{
		try
		{
			GameLoop();
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

	private bool GameLoop()
	{
		renderer.SetupConsole();

		while (!inputHandler.HasPressedEscape)
		{
			// introduction screen
			new IntroductionState(renderer, inputHandler);
			if (inputHandler.HasPressedEscape) return true;

			// ship placement
			new PlayerPlacementState(renderer, inputHandler, playerBoard);
			if (inputHandler.HasPressedEscape) return true;
			new EnemyPlacementState(renderer, enemyBoard);

			// shooting phase
			var shootingPhase = new ShootingPhaseState(renderer, inputHandler, playerBoard, enemyBoard);
			if (shootingPhase.Run()) return true;

			// game over
			new GameOverState(renderer, inputHandler, playerBoard, enemyBoard);
		}

		return false;
	}
}