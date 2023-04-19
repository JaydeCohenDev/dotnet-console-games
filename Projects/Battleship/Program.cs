using System;
using Battleship.States;

namespace Battleship;

public class Program
{
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
		Program program = new Program();
		program.Start();
	}

	public Program()
	{
		Board playerBoard = new Board(10, 10);
		Board enemyBoard = new Board(10, 10);
		
		_inputHandler = new InputHandler();
		
		_introductionState = new IntroductionState(_inputHandler);
		_playerPlacementState = new PlayerPlacementState(_inputHandler, playerBoard);
		_enemyPlacementState = new EnemyPlacementState(enemyBoard);
		_shootingPhaseState = new ShootingPhaseState(_inputHandler, playerBoard, enemyBoard, _playerPlacementState);
		_gameOverState = new GameOverState(_inputHandler, playerBoard, enemyBoard);
		_shutdownState = new ShutdownState(_exception);
		
		_renderer = new GameRenderer(playerBoard, enemyBoard, _playerPlacementState, _shootingPhaseState);
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

	private void GameLoop()
	{
		_renderer.SetupConsole();

		while (!_inputHandler.HasPressedEscape)
		{
			// introduction screen
			_introductionState.Enter();
			if (_inputHandler.HasPressedEscape) return;

			// ship placement
			_playerPlacementState.Enter();
			if (_inputHandler.HasPressedEscape) return;
			_enemyPlacementState.Enter();

			// shooting phase
			if (_shootingPhaseState.Enter()) return;

			// game over
			_gameOverState.Enter();
		}
	}
}