using System;
using Towel;

namespace Battleship;

public class PlayerPlacementState
{
	public PlayerPlacementState()
	{
		Console.Clear();
		PlaceDefenseShips();
	}
	
	private void PlaceDefenseShips()
	{
		Program.isPlacing = true;
		foreach (Ship ship in Enum.GetValues<Ship>())
		{
			Program.renderMessage = () =>
			{
				Console.WriteLine();
				Console.WriteLine($"  Place your {ship} on the grid.");
				Console.WriteLine();
				Console.WriteLine("  Use arrow keys to move the ship.");
				Console.WriteLine("  Use [spacebar] to rotate the ship.");
				Console.WriteLine("  Use [enter] to place the ship in a valid location.");
			};

			int size = (int)ship.GetTag("size").Value!;
			Program.currentPlacement = new Placement(ship, size, 0, 0, true);
			while (true)
			{
				Program.RenderMainView();
				switch (Console.ReadKey(true).Key)
				{
					case ConsoleKey.UpArrow:
						Program.currentPlacement.Row = Math.Max(Program.currentPlacement.Row - 1, 0);
						break;
					case ConsoleKey.DownArrow:
						Program.currentPlacement.Row = Math.Min(Program.currentPlacement.Row + 1, Program.playerBoard.Height - (Program.currentPlacement.Vertical ? size : 1));
						Program.currentPlacement.Column = Math.Min(Program.currentPlacement.Column, Program.playerBoard.Width - (!Program.currentPlacement.Vertical ? size : 1));
						break;
					case ConsoleKey.LeftArrow:
						Program.currentPlacement.Column = Math.Max(Program.currentPlacement.Column - 1, 0);
						break;
					case ConsoleKey.RightArrow:
						Program.currentPlacement.Row = Math.Min(Program.currentPlacement.Row, Program.playerBoard.Height - (Program.currentPlacement.Vertical ? size : 1));
						Program.currentPlacement.Column = Math.Min(Program.currentPlacement.Column + 1, Program.playerBoard.Width - (!Program.currentPlacement.Vertical ? size : 1));
						break;
					case ConsoleKey.Spacebar:
						Program.currentPlacement.Vertical = !Program.currentPlacement.Vertical;
						Program.currentPlacement.Row    = Math.Min(Program.currentPlacement.Row, Program.playerBoard.Height - (Program.currentPlacement.Vertical ? size : 1));
						Program.currentPlacement.Column = Math.Min(Program.currentPlacement.Column, Program.playerBoard.Width  - (!Program.currentPlacement.Vertical ? size : 1));
						break;
					case ConsoleKey.Enter:
						if (Program.IsValidPlacement())
						{
							for (int i = 0; i < Program.currentPlacement.Size; i++)
							{
								var row = Program.currentPlacement.Row + (Program.currentPlacement.Vertical ? i : 0);
								var col = Program.currentPlacement.Column + (!Program.currentPlacement.Vertical ? i : 0);
								Program.playerBoard.PlaceShip(ship, row, col);
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
		Program.isPlacing = false;
	}
}