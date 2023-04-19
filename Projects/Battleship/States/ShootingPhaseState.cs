using System;
using Towel.DataStructures;

namespace Battleship.States;

public class ShootingPhaseState
{
	private readonly GameRenderer _renderer;
	private readonly InputHandler _inputHandler;
	private readonly Board _playerBoard;
	private readonly Board _enemyBoard;
	public static GridPoint gridSelection;
	public static bool isSelecting;

	public ShootingPhaseState(GameRenderer renderer, InputHandler inputHandler, Board playerBoard, Board enemyBoard)
	{
		_renderer = renderer;
		_inputHandler = inputHandler;
		_playerBoard = playerBoard;
		_enemyBoard = enemyBoard;
	}
	
	public bool Run()
	{
		gridSelection = new GridPoint(_playerBoard.Height / 2, _playerBoard.Width / 2);
		Console.Clear();
		_renderer.RenderMessage = () =>
		{
			Console.WriteLine();
			Console.WriteLine("  Choose your shots.");
			Console.WriteLine();
			Console.WriteLine("  Hit: ##");
			Console.WriteLine("  Miss: XX");
			Console.WriteLine("  Use arrow keys to aim.");
			Console.WriteLine("  Use [enter] to fire at the location.");
		};
		isSelecting = true;
		while (HasNoWinnerYet())
		{
			ChooseOffense();
			if (_inputHandler.HasPressedEscape) return true;
			RandomlyChooseDefense();
			_renderer.RenderMainView();
		}

		isSelecting = false;
		return false;
	}
	
	private bool HasNoWinnerYet()
	{
		return !_playerBoard.HasWon() && !_enemyBoard.HasWon();
	}
	
	private void RandomlyChooseDefense()
	{
		if (ShouldShootPlayerShip())
		{
			ShootPlayerShip();
			return;
		}
			
		ShootOpenLocation();
	}

	private static bool ShouldShootPlayerShip()
	{
		return Random.Shared.Next(9) is 0;
	}

	private void ShootOpenLocation()
	{
		var openLocations = FindOpenLocations();
		var point = openLocations[Random.Shared.Next(openLocations.Count)];
		_playerBoard.ShootAt(point.Row, point.Column);
	}

	private ListArray<GridPoint> FindOpenLocations()
	{
		ListArray<GridPoint> openLocations = new();
		for (int row = 0; row < _playerBoard.Height; row++)
		{
			for (int col = 0; col < _playerBoard.Height; col++)
			{
				if (!_playerBoard.HasShotAt(row, col)) 
					openLocations.Add(new GridPoint(row, col));
			}
		}

		return openLocations;
	}

	private void ShootPlayerShip()
	{
		for (int row = 0; row < _playerBoard.Height; row++)
		{
			for (int col = 0; col < _playerBoard.Height; col++)
			{
				if (IsInvalidTargetLoc(row, col)) continue;
				_playerBoard.ShootAt(row, col);
				return;
			}
		}
	}

	private bool IsInvalidTargetLoc(int row, int col)
	{
		return _playerBoard.HasShotAt(row, col) || _playerBoard.GetShipAt(row, col) is 0;
	}

	private void ChooseOffense()
	{
		while (true)
		{
			_renderer.RenderMainView();
			switch (Console.ReadKey(true).Key)
			{
				case ConsoleKey.UpArrow:
					gridSelection.Row = Math.Max(0, gridSelection.Row - 1);
					break;
				case ConsoleKey.DownArrow:
					gridSelection.Row = Math.Min(_enemyBoard.Height - 1, gridSelection.Row + 1);
					break;
				case ConsoleKey.LeftArrow:
					gridSelection.Column = Math.Max(0, gridSelection.Column - 1);
					break;
				case ConsoleKey.RightArrow:
					gridSelection.Column = Math.Min(_enemyBoard.Width - 1, gridSelection.Column + 1);
					break;
				case ConsoleKey.Enter:
					if (!_enemyBoard.HasShotAt(gridSelection.Row, gridSelection.Column))
					{
						_enemyBoard.ShootAt(gridSelection.Row, gridSelection.Column);
						PlayerPlacementState.IsPlacing = false;
						return;
					}
					break;
				case ConsoleKey.Escape:
					_inputHandler.HasPressedEscape = true;
					PlayerPlacementState.IsPlacing = false;
					return;
			}
		}
	}
}