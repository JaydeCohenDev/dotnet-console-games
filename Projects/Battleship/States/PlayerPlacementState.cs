using System;
using Towel;

namespace Battleship.States;

public class PlayerPlacementState
{
	private readonly GameRenderer _renderer;
	private readonly InputHandler _inputHandler;
	private readonly Board _playerBoard;
	public static bool isPlacing;
	public static Placement currentPlacement;
	
	public PlayerPlacementState(GameRenderer renderer, InputHandler inputHandler, Board playerBoard)
	{
		_renderer = renderer;
		_inputHandler = inputHandler;
		_playerBoard = playerBoard;
		Console.Clear();
		PlaceDefenseShips();
	}
	
	private void PlaceDefenseShips()
	{
		isPlacing = true;
		foreach (Ship ship in Enum.GetValues<Ship>())
		{
			UpdateRenderMessage(ship);

			int size = (int)ship.GetTag("size").Value!;
			currentPlacement = new Placement(ship, size, 0, 0, true);
			bool isPlacementMode = true;
			
			while (isPlacementMode)
			{
				_renderer.RenderMainView();
				if (HandlePlacement(size, ship, ref isPlacementMode)) return;
			}
		}
		isPlacing = false;
	}

	private void UpdateRenderMessage(Ship ship)
	{
		_renderer.RenderMessage = () =>
		{
			Console.WriteLine();
			Console.WriteLine($"  Place your {ship} on the grid.");
			Console.WriteLine();
			Console.WriteLine("  Use arrow keys to move the ship.");
			Console.WriteLine("  Use [spacebar] to rotate the ship.");
			Console.WriteLine("  Use [enter] to place the ship in a valid location.");
		};
	}

	private bool HandlePlacement(int size, Ship ship, ref bool isPlacementMode)
	{
		switch (Console.ReadKey(true).Key)
		{
			case ConsoleKey.UpArrow:
				MoveUp();
				break;
			case ConsoleKey.DownArrow:
				MoveDown(size);
				break;
			case ConsoleKey.LeftArrow:
				MoveLeft();
				break;
			case ConsoleKey.RightArrow:
				MoveRight(size);
				break;
			case ConsoleKey.Spacebar:
				RotatePlacement(size);
				break;
			case ConsoleKey.Enter:
				isPlacementMode = PlaceShip(ship, isPlacementMode);
				break;
			case ConsoleKey.Escape:
				Exit();
				return true;
		}

		return false;
	}

	private void Exit()
	{
		_inputHandler.HasPressedEscape = true;
	}

	private bool PlaceShip(Ship ship, bool isPlacementMode)
	{
		if (!_playerBoard.IsValidPlacement(currentPlacement)) return isPlacementMode;
		for (int i = 0; i < currentPlacement.Size; i++)
		{
			int row = currentPlacement.Row + (currentPlacement.Vertical ? i : 0);
			int col = currentPlacement.Column + (!currentPlacement.Vertical ? i : 0);
			_playerBoard.PlaceShip(ship, row, col);
		}

		isPlacementMode = false;

		return isPlacementMode;
	}

	private void RotatePlacement(int size)
	{
		currentPlacement.Vertical = !currentPlacement.Vertical;
		currentPlacement.Row = Math.Min(currentPlacement.Row, _playerBoard.Height - (currentPlacement.Vertical ? size : 1));
		currentPlacement.Column =
			Math.Min(currentPlacement.Column, _playerBoard.Width - (!currentPlacement.Vertical ? size : 1));
	}

	private void MoveRight(int size)
	{
		currentPlacement.Row = Math.Min(currentPlacement.Row, _playerBoard.Height - (currentPlacement.Vertical ? size : 1));
		currentPlacement.Column = Math.Min(currentPlacement.Column + 1,
			_playerBoard.Width - (!currentPlacement.Vertical ? size : 1));
	}

	private static void MoveLeft()
	{
		currentPlacement.Column = Math.Max(currentPlacement.Column - 1, 0);
	}

	private void MoveDown(int size)
	{
		currentPlacement.Row =
			Math.Min(currentPlacement.Row + 1, _playerBoard.Height - (currentPlacement.Vertical ? size : 1));
		currentPlacement.Column =
			Math.Min(currentPlacement.Column, _playerBoard.Width - (!currentPlacement.Vertical ? size : 1));
	}

	private static void MoveUp()
	{
		currentPlacement.Row = Math.Max(currentPlacement.Row - 1, 0);
	}
}