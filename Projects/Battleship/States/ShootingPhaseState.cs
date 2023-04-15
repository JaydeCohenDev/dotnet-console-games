using System;
using Towel.DataStructures;

namespace Battleship;

public class ShootingPhaseState
{
	private readonly GameRenderer renderer;
	private readonly InputHandler inputHandler;
	private readonly Board playerBoard;
	private readonly Board enemyBoard;
	public static GridPoint gridSelection;
	public static bool isSelecting;

	public ShootingPhaseState(GameRenderer renderer, InputHandler inputHandler, Board playerBoard, Board enemyBoard)
	{
		this.renderer = renderer;
		this.inputHandler = inputHandler;
		this.playerBoard = playerBoard;
		this.enemyBoard = enemyBoard;
	}
	
	public bool Run()
	{
		gridSelection = new GridPoint(playerBoard.Height / 2, playerBoard.Width / 2);
		Console.Clear();
		renderer.renderMessage = () =>
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
			if (inputHandler.HasPressedEscape) return true;
			RandomlyChooseDefense();
			renderer.RenderMainView();
		}

		isSelecting = false;
		return false;
	}
	
	private bool HasNoWinnerYet()
	{
		return !playerBoard.HasWon() && !enemyBoard.HasWon();
	}
	
	private void RandomlyChooseDefense()
	{
		if (Random.Shared.Next(9) is 0)
		{
			for (int r = 0; r < playerBoard.Height; r++)
			{
				for (int c = 0; c < playerBoard.Height; c++)
				{
					if (!playerBoard.HasShotAt(r, c) && playerBoard.GetShipAt(r, c) is not 0)
					{
						playerBoard.ShootAt(r, c);
						return;
					}
				}
			}
		}
		else
		{
			ListArray<(int Row, int Column)> openLocations = new();
			for (int r = 0; r < playerBoard.Height; r++)
			{
				for (int c = 0; c < playerBoard.Height; c++)
				{
					if (!playerBoard.HasShotAt(r, c))
					{
						openLocations.Add((r, c));
					}
				}
			}
			(int row, int column) = openLocations[Random.Shared.Next(openLocations.Count)];
			playerBoard.ShootAt(row, column);
		}
	}
	
	private void ChooseOffense()
	{
		while (true)
		{
			renderer.RenderMainView();
			switch (Console.ReadKey(true).Key)
			{
				case ConsoleKey.UpArrow:
					gridSelection.Row = Math.Max(0, gridSelection.Row - 1);
					break;
				case ConsoleKey.DownArrow:
					gridSelection.Row = Math.Min(enemyBoard.Height - 1, gridSelection.Row + 1);
					break;
				case ConsoleKey.LeftArrow:
					gridSelection.Column = Math.Max(0, gridSelection.Column - 1);
					break;
				case ConsoleKey.RightArrow:
					gridSelection.Column = Math.Min(enemyBoard.Width - 1, gridSelection.Column + 1);
					break;
				case ConsoleKey.Enter:
					if (!enemyBoard.HasShotAt(gridSelection.Row, gridSelection.Column))
					{
						enemyBoard.ShootAt(gridSelection.Row, gridSelection.Column);
						PlayerPlacementState.isPlacing = false;
						return;
					}
					break;
				case ConsoleKey.Escape:
					inputHandler.HasPressedEscape = true;
					PlayerPlacementState.isPlacing = false;
					return;
			}
		}
	}
}