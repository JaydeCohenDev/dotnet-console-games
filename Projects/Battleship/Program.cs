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

	private readonly IntroductionState _introductionState;
	private readonly PlayerPlacementState _playerPlacementState;
	private readonly EnemyPlacementState _enemyPlacementState;
	private readonly ShootingPhaseState _shootingPhaseState;
	private readonly GameOverState _gameOverState;
	private readonly ShutdownState _shutdownState;
	
	public static void Main(string[] args)
	{
		var program = new Program();
		program.Start();
	}

	public Program()
	{
		_playerBoard = new Board(10, 10);
		_enemyBoard = new Board(10, 10);
		
		_inputHandler = new InputHandler();
		
		_introductionState = new IntroductionState(_inputHandler);
		_playerPlacementState = new PlayerPlacementState(_inputHandler, _playerBoard);
		_enemyPlacementState = new EnemyPlacementState(_enemyBoard);
		_shootingPhaseState = new ShootingPhaseState(_inputHandler, _playerBoard, _enemyBoard, _playerPlacementState);
		_gameOverState = new GameOverState(_inputHandler, _playerBoard, _enemyBoard);
		_shutdownState = new ShutdownState(_exception);
		
		_renderer = new GameRenderer(_playerBoard, _enemyBoard, _playerPlacementState, _shootingPhaseState);
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
			_shutdownState.Enter();
		}
	}

	private bool GameLoop()
	{
		_renderer.SetupConsole();

		while (!_inputHandler.HasPressedEscape)
		{
			// introduction screen
			_introductionState.Enter();
			if (_inputHandler.HasPressedEscape) return true;

			// ship placement
			_playerPlacementState.Enter();
			if (_inputHandler.HasPressedEscape) return true;
			_enemyPlacementState.Enter();

			// shooting phase
			if (_shootingPhaseState.Enter()) return true;

			// game over
			_gameOverState.Enter();
		}

		return false;
	}
}