using System;
using Towel.DataStructures;

namespace Battleship;

public class ShootingPhaseState
{
	public static GridPoint gridSelection;
	public static bool isSelecting;
	
	public bool Run()
	{
		gridSelection = new GridPoint(Program.playerBoard.Height / 2, Program.playerBoard.Width / 2);
		Console.Clear();
		Program.renderer.renderMessage = () =>
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
			if (Program.inputHandler.HasPressedEscape) return true;
			RandomlyChooseDefense();
			Program.renderer.RenderMainView();
		}

		isSelecting = false;
		return false;
	}
	
	private bool HasNoWinnerYet()
	{
		return !Program.playerBoard.HasWon() && !Program.enemyBoard.HasWon();
	}
	
	private void RandomlyChooseDefense()
	{
		if (Random.Shared.Next(9) is 0)
		{
			for (int r = 0; r < Program.playerBoard.Height; r++)
			{
				for (int c = 0; c < Program.playerBoard.Height; c++)
				{
					if (!Program.playerBoard.HasShotAt(r, c) && Program.playerBoard.GetShipAt(r, c) is not 0)
					{
						Program.playerBoard.ShootAt(r, c);
						return;
					}
				}
			}
		}
		else
		{
			ListArray<(int Row, int Column)> openLocations = new();
			for (int r = 0; r < Program.playerBoard.Height; r++)
			{
				for (int c = 0; c < Program.playerBoard.Height; c++)
				{
					if (!Program.playerBoard.HasShotAt(r, c))
					{
						openLocations.Add((r, c));
					}
				}
			}
			var (row, column) = openLocations[Random.Shared.Next(openLocations.Count)];
			Program.playerBoard.ShootAt(row, column);
		}
	}
	
	private void ChooseOffense()
	{
		while (true)
		{
			Program.renderer.RenderMainView();
			switch (Console.ReadKey(true).Key)
			{
				case ConsoleKey.UpArrow:
					gridSelection.Row = Math.Max(0, gridSelection.Row - 1);
					break;
				case ConsoleKey.DownArrow:
					gridSelection.Row = Math.Min(Program.enemyBoard.Height - 1, gridSelection.Row + 1);
					break;
				case ConsoleKey.LeftArrow:
					gridSelection.Column = Math.Max(0, gridSelection.Column - 1);
					break;
				case ConsoleKey.RightArrow:
					gridSelection.Column = Math.Min(Program.enemyBoard.Width - 1, gridSelection.Column + 1);
					break;
				case ConsoleKey.Enter:
					if (!Program.enemyBoard.HasShotAt(gridSelection.Row, gridSelection.Column))
					{
						Program.enemyBoard.ShootAt(gridSelection.Row, gridSelection.Column);
						PlayerPlacementState.isPlacing = false;
						return;
					}
					break;
				case ConsoleKey.Escape:
					Program.inputHandler.HasPressedEscape = true;
					PlayerPlacementState.isPlacing = false;
					return;
			}
		}
	}
}