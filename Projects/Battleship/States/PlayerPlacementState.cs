using System;
using Towel;

namespace Battleship;

public class PlayerPlacementState
{
	private readonly Board playerBoard;
	public static bool isPlacing;
	public static Placement currentPlacement;
	
	public PlayerPlacementState(Board playerBoard)
	{
		this.playerBoard = playerBoard;
		Console.Clear();
		PlaceDefenseShips();
	}
	
	private void PlaceDefenseShips()
	{
		isPlacing = true;
		foreach (Ship ship in Enum.GetValues<Ship>())
		{
			Program.renderer.renderMessage = () =>
			{
				Console.WriteLine();
				Console.WriteLine($"  Place your {ship} on the grid.");
				Console.WriteLine();
				Console.WriteLine("  Use arrow keys to move the ship.");
				Console.WriteLine("  Use [spacebar] to rotate the ship.");
				Console.WriteLine("  Use [enter] to place the ship in a valid location.");
			};

			int size = (int)ship.GetTag("size").Value!;
			currentPlacement = new Placement(ship, size, 0, 0, true);
			while (true)
			{
				Program.renderer.RenderMainView();
				switch (Console.ReadKey(true).Key)
				{
					case ConsoleKey.UpArrow:
						currentPlacement.Row = Math.Max(currentPlacement.Row - 1, 0);
						break;
					case ConsoleKey.DownArrow:
						currentPlacement.Row = Math.Min(currentPlacement.Row + 1, playerBoard.Height - (currentPlacement.Vertical ? size : 1));
						currentPlacement.Column = Math.Min(currentPlacement.Column, playerBoard.Width - (!currentPlacement.Vertical ? size : 1));
						break;
					case ConsoleKey.LeftArrow:
						currentPlacement.Column = Math.Max(currentPlacement.Column - 1, 0);
						break;
					case ConsoleKey.RightArrow:
						currentPlacement.Row = Math.Min(currentPlacement.Row, playerBoard.Height - (currentPlacement.Vertical ? size : 1));
						currentPlacement.Column = Math.Min(currentPlacement.Column + 1, playerBoard.Width - (!currentPlacement.Vertical ? size : 1));
						break;
					case ConsoleKey.Spacebar:
						currentPlacement.Vertical = !currentPlacement.Vertical;
						currentPlacement.Row    = Math.Min(currentPlacement.Row, playerBoard.Height - (currentPlacement.Vertical ? size : 1));
						currentPlacement.Column = Math.Min(currentPlacement.Column, playerBoard.Width  - (!currentPlacement.Vertical ? size : 1));
						break;
					case ConsoleKey.Enter:
						if (Program.playerBoard.IsValidPlacement(currentPlacement))
						{
							for (int i = 0; i < currentPlacement.Size; i++)
							{
								var row = currentPlacement.Row + (currentPlacement.Vertical ? i : 0);
								var col = currentPlacement.Column + (!currentPlacement.Vertical ? i : 0);
								playerBoard.PlaceShip(ship, row, col);
							}
							goto Continue;
						}
						break;
					case ConsoleKey.Escape:
						Program.inputHandler.hasPressedEscape = true;
						return;
				}
			}
		Continue:
			continue;
		}
		isPlacing = false;
	}
}