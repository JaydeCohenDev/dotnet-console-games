using System;
using Battleship.States;

namespace Battleship;

public class Program
{
	private readonly Board _playerBoard;
	private readonly Board _enemyBoard;
	private readonly InputHandler _inputHandler;
	private readonly GameRenderer _renderer;
	private Exception? _exception;
	
	public static void Main(string[] args)
	{
		var program = new Program();
		program.Start();
	}

	public Program()
	{
		_playerBoard = new Board(10, 10);
		_enemyBoard = new Board(10, 10);

		_renderer = new GameRenderer(_playerBoard, _enemyBoard);
		_inputHandler = new InputHandler();
	}

	public void Start()
	{
		try
		{
			GameLoop();
		}
		catch (Exception ex)
		{
			_exception = ex;
			throw;
		}
		finally
		{
			new ShutdownState(_exception);
		}
	}

	private bool GameLoop()
	{
		_renderer.SetupConsole();

		while (!_inputHandler.HasPressedEscape)
		{
			// introduction screen
			new IntroductionState(_renderer, _inputHandler);
			if (_inputHandler.HasPressedEscape) return true;

			// ship placement
			new PlayerPlacementState(_renderer, _inputHandler, _playerBoard);
			if (_inputHandler.HasPressedEscape) return true;
			new EnemyPlacementState(_renderer, _enemyBoard);

			// shooting phase
			var shootingPhase = new ShootingPhaseState(_renderer, _inputHandler, _playerBoard, _enemyBoard);
			if (shootingPhase.Run()) return true;

			// game over
			new GameOverState(_renderer, _inputHandler, _playerBoard, _enemyBoard);
		}

		return false;
	}
}