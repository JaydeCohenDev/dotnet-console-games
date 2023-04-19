using System;
using Towel.DataStructures;

namespace Battleship.States;

public class ShootingPhaseState : IGameState
{
	private readonly InputHandler _inputHandler;
	private readonly Board _playerBoard;
	private readonly Board _enemyBoard;
	private readonly PlayerPlacementState _playerPlacementState;
	public GridPoint GridSelection;
	public bool IsSelecting;

	public ShootingPhaseState(InputHandler inputHandler, Board playerBoard, Board enemyBoard, PlayerPlacementState playerPlacementState)
	{
		_inputHandler = inputHandler;
		_playerBoard = playerBoard;
		_enemyBoard = enemyBoard;
		_playerPlacementState = playerPlacementState;
	}

	public bool Enter()
	{
		return Run();
	}
	
	public bool Run()
	{
		GridSelection = new GridPoint(_playerBoard.Height / 2, _playerBoard.Width / 2);
		Console.Clear();
		GameRenderer.Global.RenderMessage = () =>
		{
			Console.WriteLine();
			Console.WriteLine("  Choose your shots.");
			Console.WriteLine();
			Console.WriteLine("  Hit: ##");
			Console.WriteLine("  Miss: XX");
			Console.WriteLine("  Use arrow keys to aim.");
			Console.WriteLine("  Use [enter] to fire at the location.");
		};
		IsSelecting = true;
		while (HasNoWinnerYet())
		{
			ChooseOffense();
			if (_inputHandler.HasPressedEscape) return true;
			RandomlyChooseDefense();
			GameRenderer.Global.RenderMainView();
		}

		IsSelecting = false;
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

	private bool ShouldShootPlayerShip()
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
			GameRenderer.Global.RenderMainView();
			switch (Console.ReadKey(true).Key)
			{
				case ConsoleKey.UpArrow:
					GridSelection.Row = Math.Max(0, GridSelection.Row - 1);
					break;
				case ConsoleKey.DownArrow:
					GridSelection.Row = Math.Min(_enemyBoard.Height - 1, GridSelection.Row + 1);
					break;
				case ConsoleKey.LeftArrow:
					GridSelection.Column = Math.Max(0, GridSelection.Column - 1);
					break;
				case ConsoleKey.RightArrow:
					GridSelection.Column = Math.Min(_enemyBoard.Width - 1, GridSelection.Column + 1);
					break;
				case ConsoleKey.Enter:
					if (!_enemyBoard.HasShotAt(GridSelection.Row, GridSelection.Column))
					{
						_enemyBoard.ShootAt(GridSelection.Row, GridSelection.Column);
						_playerPlacementState.IsPlacing = false;
						return;
					}
					break;
				case ConsoleKey.Escape:
					_inputHandler.HasPressedEscape = true;
					_playerPlacementState.IsPlacing = false;
					return;
			}
		}
	}

	public void Render(GameRenderer renderer)
	{
		throw new NotImplementedException();
	}
}