using System;
using Towel;

namespace Battleship.States;

public class PlayerPlacementState : IGameState
{
	private readonly InputHandler _inputHandler;
	private readonly Board _playerBoard;
	public bool IsPlacing;
	public Placement CurrentPlacement;
	
	public PlayerPlacementState(InputHandler inputHandler, Board playerBoard)
	{
		_inputHandler = inputHandler;
		_playerBoard = playerBoard;
	}

	public bool Enter()
	{
		Console.Clear();
		PlaceDefenseShips();
		return false;
	}
	
	private void PlaceDefenseShips()
	{
		IsPlacing = true;
		foreach (Ship ship in Enum.GetValues<Ship>())
		{
			UpdateRenderMessage(ship);

			int size = (int)ship.GetTag("size").Value!;
			CurrentPlacement = new Placement(ship, size, 0, 0, true);
			bool isPlacementMode = true;
			
			while (isPlacementMode)
			{
				GameRenderer.Global.RenderMainView();
				if (HandlePlacement(size, ship, ref isPlacementMode)) return;
			}
		}
		IsPlacing = false;
	}

	private void UpdateRenderMessage(Ship ship)
	{
		GameRenderer.Global.RenderMessage = () =>
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
		if (!_playerBoard.IsValidPlacement(CurrentPlacement)) return isPlacementMode;
		for (int i = 0; i < CurrentPlacement.Size; i++)
		{
			int row = CurrentPlacement.Row + (CurrentPlacement.Vertical ? i : 0);
			int col = CurrentPlacement.Column + (!CurrentPlacement.Vertical ? i : 0);
			_playerBoard.PlaceShip(ship, row, col);
		}

		isPlacementMode = false;

		return isPlacementMode;
	}

	private void RotatePlacement(int size)
	{
		CurrentPlacement.Vertical = !CurrentPlacement.Vertical;
		CurrentPlacement.Row = Math.Min(CurrentPlacement.Row, _playerBoard.Height - (CurrentPlacement.Vertical ? size : 1));
		CurrentPlacement.Column =
			Math.Min(CurrentPlacement.Column, _playerBoard.Width - (!CurrentPlacement.Vertical ? size : 1));
	}

	private void MoveRight(int size)
	{
		CurrentPlacement.Row = Math.Min(CurrentPlacement.Row, _playerBoard.Height - (CurrentPlacement.Vertical ? size : 1));
		CurrentPlacement.Column = Math.Min(CurrentPlacement.Column + 1,
			_playerBoard.Width - (!CurrentPlacement.Vertical ? size : 1));
	}

	private void MoveLeft()
	{
		CurrentPlacement.Column = Math.Max(CurrentPlacement.Column - 1, 0);
	}

	private void MoveDown(int size)
	{
		CurrentPlacement.Row =
			Math.Min(CurrentPlacement.Row + 1, _playerBoard.Height - (CurrentPlacement.Vertical ? size : 1));
		CurrentPlacement.Column =
			Math.Min(CurrentPlacement.Column, _playerBoard.Width - (!CurrentPlacement.Vertical ? size : 1));
	}

	private void MoveUp()
	{
		CurrentPlacement.Row = Math.Max(CurrentPlacement.Row - 1, 0);
	}

	public void Render(GameRenderer renderer)
	{
		
	}
}