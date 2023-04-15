using System;
using Towel;
using Towel.DataStructures;

public static class Program
{
	private static Exception? exception;
	private const int BoardHeight = 10;
	private const int BoardWidth = 10;
	private static bool[,] enemyBoardShots;
	private static Ship[,] enemyBoardShips;
	private static bool[,] playerBoardShots;
	private static Ship[,] playerBoardShips;
	private static (int BufferHeight, int BufferWidth, int WindowHeight, int WindowWidth) consoleSize;
	private static bool isPlacing;
	private static (Ship Ship, int Size, int Row, int Column, bool Vertical) currentPlacement;
	private static bool hasPressedEscape;
	private static (int Row, int Column) gridSelection;
	private static bool isSelecting;
	private static Action? renderMessage;
	
	public static void Main(string[] args)
	{
		try
		{
			Console.BackgroundColor = ConsoleColor.Black;
			Console.ForegroundColor = ConsoleColor.White;
			Console.Clear();
			consoleSize = ConsoleSize();

			while (!hasPressedEscape)
			{
				enemyBoardShots = new bool[BoardHeight, BoardWidth];
				enemyBoardShips = new Ship[BoardHeight, BoardWidth];
				playerBoardShots = new bool[BoardHeight, BoardWidth];
				playerBoardShips = new Ship[BoardHeight, BoardWidth];

				// introduction screen
				Console.Clear();
				renderMessage = () =>
				{
					Console.WriteLine();
					Console.WriteLine("  This is a guessing game where you will place your battle ships");
					Console.WriteLine("  on a grid, and then shoot locations of the enemy grid trying");
					Console.WriteLine("  to find and sink all of their ships.The first player to sink");
					Console.WriteLine("  all the enemy ships wins.");
					Console.WriteLine();
					Console.WriteLine("  Press [escape] at any time to close the game.");
					Console.WriteLine();
					Console.WriteLine("  Press [enter] to begin...");
				};
				RenderMainView();
				GetEnterOrEscape();
				if (hasPressedEscape)
				{
					return;
				}

				// ship placement
				Console.Clear();
				PlaceDefenseShips();
				if (hasPressedEscape)
				{
					return;
				}
				RandomizeOffenseShips();
				renderMessage = () =>
				{
					Console.WriteLine();
					Console.WriteLine("  The enemy has placed their ships.");
					Console.WriteLine();
					Console.WriteLine("  Press [enter] to continue...");
				};
				RenderMainView();

				// shooting phase
				gridSelection = (BoardHeight / 2, BoardWidth / 2);
				Console.Clear();
				renderMessage = () =>
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
				while (!Won(playerBoardShips, playerBoardShots) && !Won(enemyBoardShips, enemyBoardShots))
				{
					ChooseOffense();
					if (hasPressedEscape)
					{
						return;
					}
					RandomlyChooseDefense();
					RenderMainView();
				}
				isSelecting = false;

				// game over
				Console.Clear();
				renderMessage = () =>
				{
					Console.WriteLine();
					switch ((Won(playerBoardShips, playerBoardShots), Won(enemyBoardShips, enemyBoardShots)))
					{
						case (true, true):
							Console.WriteLine("  Draw! All ships were sunk.");
							break;
						case (false, true):
							Console.WriteLine("  You Win! You sunk all the enemy ships.");
							break;
						case (true, false):
							Console.WriteLine("  You Lose! The enemy sunk all your ships.");
							break;
					}
					Console.WriteLine();
					Console.WriteLine("  Play again [enter] or quit [escape]?");
				};
				RenderMainView(showEnemyShips: true);
				GetEnterOrEscape();
			}
		}
		catch (Exception ex)
		{
			exception = ex;
			throw;
		}
		finally
		{
			Console.CursorVisible = true;
			Console.ResetColor();
			Console.Clear();
			Console.WriteLine(exception?.ToString() ?? "Battleship was closed.");
		}

	}


	private static void PlaceDefenseShips()
	{
		isPlacing = true;
		foreach (Ship ship in Enum.GetValues<Ship>())
		{
			renderMessage = () =>
			{
				Console.WriteLine();
				Console.WriteLine($"  Place your {ship} on the grid.");
				Console.WriteLine();
				Console.WriteLine("  Use arrow keys to move the ship.");
				Console.WriteLine("  Use [spacebar] to rotate the ship.");
				Console.WriteLine("  Use [enter] to place the ship in a valid location.");
			};

			int size = (int)ship.GetTag("size").Value!;
			currentPlacement = (ship, size, 0, 0, true);
			while (true)
			{
				RenderMainView();
				switch (Console.ReadKey(true).Key)
				{
					case ConsoleKey.UpArrow:
						currentPlacement.Row = Math.Max(currentPlacement.Row - 1, 0);
						break;
					case ConsoleKey.DownArrow:
						currentPlacement.Row = Math.Min(currentPlacement.Row + 1, BoardHeight - (currentPlacement.Vertical ? size : 1));
						currentPlacement.Column = Math.Min(currentPlacement.Column, BoardWidth - (!currentPlacement.Vertical ? size : 1));
						break;
					case ConsoleKey.LeftArrow:
						currentPlacement.Column = Math.Max(currentPlacement.Column - 1, 0);
						break;
					case ConsoleKey.RightArrow:
						currentPlacement.Row = Math.Min(currentPlacement.Row, BoardHeight - (currentPlacement.Vertical ? size : 1));
						currentPlacement.Column = Math.Min(currentPlacement.Column + 1, BoardWidth - (!currentPlacement.Vertical ? size : 1));
						break;
					case ConsoleKey.Spacebar:
						currentPlacement.Vertical = !currentPlacement.Vertical;
						currentPlacement.Row    = Math.Min(currentPlacement.Row, BoardHeight - (currentPlacement.Vertical ? size : 1));
						currentPlacement.Column = Math.Min(currentPlacement.Column, BoardWidth  - (!currentPlacement.Vertical ? size : 1));
						break;
					case ConsoleKey.Enter:
						if (IsValidPlacement())
						{
							for (int i = 0; i < currentPlacement.Size; i++)
							{
								playerBoardShips[currentPlacement.Row + (currentPlacement.Vertical ? i : 0), currentPlacement.Column + (!currentPlacement.Vertical ? i : 0)] = ship;
							}
							goto Continue;
						}
						break;
					case ConsoleKey.Escape:
						hasPressedEscape = true;
						return;
				}
			}
		Continue:
			continue;
		}
		isPlacing = false;
	}

	private static void ChooseOffense()
	{
		while (true)
		{
			RenderMainView();
			switch (Console.ReadKey(true).Key)
			{
				case ConsoleKey.UpArrow:
					gridSelection.Row = Math.Max(0, gridSelection.Row - 1);
					break;
				case ConsoleKey.DownArrow:
					gridSelection.Row = Math.Min(BoardHeight - 1, gridSelection.Row + 1);
					break;
				case ConsoleKey.LeftArrow:
					gridSelection.Column = Math.Max(0, gridSelection.Column - 1);
					break;
				case ConsoleKey.RightArrow:
					gridSelection.Column = Math.Min(BoardWidth - 1, gridSelection.Column + 1);
					break;
				case ConsoleKey.Enter:
					if (!enemyBoardShots[gridSelection.Row, gridSelection.Column])
					{
						enemyBoardShots[gridSelection.Row, gridSelection.Column] = true;
						isPlacing = false;
						return;
					}
					break;
				case ConsoleKey.Escape:
					hasPressedEscape = true;
					isPlacing = false;
					return;
			}
		}
	}

	private static void RandomlyChooseDefense()
	{
		if (Random.Shared.Next(9) is 0)
		{
			for (int r = 0; r < BoardHeight; r++)
			{
				for (int c = 0; c < BoardHeight; c++)
				{
					if (!playerBoardShots[r, c] && playerBoardShips[r, c] is not 0)
					{
						playerBoardShots[r, c] = true;
						return;
					}
				}
			}
		}
		else
		{
			ListArray<(int Row, int Column)> openLocations = new();
			for (int r = 0; r < BoardHeight; r++)
			{
				for (int c = 0; c < BoardHeight; c++)
				{
					if (!playerBoardShots[r, c])
					{
						openLocations.Add((r, c));
					}
				}
			}
			var (row, column) = openLocations[Random.Shared.Next(openLocations.Count)];
			playerBoardShots[row, column] = true;
		}
	}

	private static bool IsValidPlacement()
	{
		for (int i = 0; i < currentPlacement.Size; i++)
		{
			if (playerBoardShips[currentPlacement.Row + (currentPlacement.Vertical ? i : 0), currentPlacement.Column + (!currentPlacement.Vertical ? i : 0)] is not 0)
			{
				return false;
			}
		}
		return true;
	}

	private static void RandomizeOffenseShips()
	{
		foreach (Ship ship in Enum.GetValues<Ship>())
		{
			int size = (int)ship.GetTag("size").Value!;
			ListArray<(int Row, int Column, bool Vertical)> locations = new();
			for (int row = 0; row < BoardHeight - size; row++)
			{
				for (int col = 0; col < BoardWidth; col++)
				{
					bool vertical = true;
					bool horizontal = true;
					for (int i = 0; i < size; i++)
					{
						if (row + size > BoardHeight || enemyBoardShips[row + i, col] is not 0)
						{
							vertical = false;
						}
						if (col + size > BoardWidth || enemyBoardShips[row, col + i] is not 0)
						{
							horizontal = false;
						}
					}
					if (vertical)
					{
						locations.Add((row, col, true));
					}
					if (horizontal)
					{
						locations.Add((row, col, false));
					}
				}
			}
			var (Row, Column, Vertical) = locations[Random.Shared.Next(0, locations.Count)];
			for (int i = 0; i < size; i++)
			{
				enemyBoardShips[Row + (Vertical ? i : 0), Column + (!Vertical ? i : 0)] = ship;
			}
		}
	}

	private static bool Won(Ship[,] shipBoard, bool[,] shotBoard)
	{
		for (int row = 0; row < BoardHeight; row++)
		{
			for (int col = 0; col < BoardWidth; col++)
			{
				if (shipBoard[row, col] is not 0 && !shotBoard[row, col])
				{
					return false;
				}
			}
		}
		return true;
	}

	private static void RenderMainView(bool showEnemyShips = false)
	{
		Console.CursorVisible = false;
		if (OperatingSystem.IsWindows() && Console.BufferHeight != Console.WindowHeight)
		{
			Console.BufferHeight = Console.WindowHeight;
		}
		if (OperatingSystem.IsWindows() && Console.BufferWidth != Console.WindowWidth)
		{
			Console.BufferWidth = Console.WindowWidth;
		}
		if (consoleSize != ConsoleSize())
		{
			Console.Clear();
			consoleSize = ConsoleSize();
		}

		Console.SetCursorPosition(0, 0);
		Console.WriteLine();
		Console.WriteLine("  Battleship");
		Console.WriteLine();
		for (int row = 0; row < BoardHeight * 2 + 1; row++)
		{
			int boardRow = (row - 1) / 2;
			Console.Write("  ");
			for (int col = 0; col < BoardWidth * 2 + 1; col++)
			{
				int boardCol = (col - 1) / 2;
				bool v = boardRow + 1 < BoardHeight && playerBoardShips[boardRow, boardCol] == playerBoardShips[boardRow + 1, boardCol];
				bool h = boardCol + 1 < BoardWidth && playerBoardShips[boardRow, boardCol] == playerBoardShips[boardRow, boardCol + 1];

				if (isPlacing &&
					currentPlacement.Vertical &&
					boardCol == currentPlacement.Column &&
					boardRow >= currentPlacement.Row &&
					boardRow < currentPlacement.Row + currentPlacement.Size &&
					(col - 1) % 2 is 0 &&
					!(boardRow == currentPlacement.Row + currentPlacement.Size - 1 && (row - 1) % 2 is 1) &&
					row is not 0)
				{
					Console.BackgroundColor = IsValidPlacement() ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
				}
				else if (isPlacing &&
					!currentPlacement.Vertical &&
					boardRow == currentPlacement.Row &&
					boardCol >= currentPlacement.Column &&
					boardCol < currentPlacement.Column + currentPlacement.Size &&
					(row - 1) % 2 is 0 &&
					!(boardCol == currentPlacement.Column + currentPlacement.Size - 1 && (col - 1) % 2 is 1) &&
					col is not 0)
				{
					Console.BackgroundColor = IsValidPlacement() ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
				}
				else if (playerBoardShips[boardRow, boardCol] is not 0 &&
					((row - 1) % 2 is 0 || ((row - 1) % 2 is 1 && v)) &&
					((col - 1) % 2 is 0 || ((col - 1) % 2 is 1 && h)))
				{
					Console.BackgroundColor = ConsoleColor.DarkGray;
				}
				Console.Write(RenderBoardTile(row, col, playerBoardShots, playerBoardShips));
				if (Console.BackgroundColor is not ConsoleColor.Black)
				{
					Console.BackgroundColor = ConsoleColor.Black;
				}
			}
			Console.Write("  ");
			for (int col = 0; col < BoardWidth * 2 + 1; col++)
			{
				int boardCol = (col - 1) / 2;
				bool v = boardRow + 1 < BoardHeight && enemyBoardShips[boardRow, boardCol] == enemyBoardShips[boardRow + 1, boardCol];
				bool h = boardCol + 1 < BoardWidth && enemyBoardShips[boardRow, boardCol] == enemyBoardShips[boardRow, boardCol + 1];
				if (showEnemyShips &&
					enemyBoardShips[boardRow, boardCol] is not 0 &&
					((row - 1) % 2 is 0 || ((row - 1) % 2 is 1 && v)) &&
					((col - 1) % 2 is 0 || ((col - 1) % 2 is 1 && h)))
				{
					Console.BackgroundColor = ConsoleColor.DarkGray;
				}
				else if (isSelecting && gridSelection == (boardRow, boardCol) &&
					(row - 1) % 2 is 0 &&
					(col - 1) % 2 is 0)
				{
					Console.BackgroundColor = ConsoleColor.DarkYellow;
				}
				Console.Write(RenderBoardTile(row, col, enemyBoardShots, enemyBoardShips));
				if (Console.BackgroundColor is not ConsoleColor.Black)
				{
					Console.BackgroundColor = ConsoleColor.Black;
				}
			}
			Console.WriteLine();
		}
		renderMessage?.Invoke();

		
	}

	private static string RenderBoardTile(int row, int col, bool[,] shots, Ship[,] ships)
	{
		const string hit = "##";
		const string miss = "XX";
		const string open = "  ";
		const int fullWidth = BoardWidth * 2;
		const int fullHeight = BoardHeight * 2;
		return (r: row, c: col, row % 2, col % 2) switch
		{
			(0, 0, _, _) => "┌",
			(fullHeight, 0, _, _) => "└",
			(0, fullWidth, _, _) => "┐",
			(fullHeight, fullWidth, _, _) => "┘",
			(0, _, 0, 0) => "┬",
			(_, 0, 0, 0) => "├",
			(_, fullWidth, 0, _) => "┤",
			(fullHeight, _, _, 0) => "┴",
			(_, _, 0, 0) => "┼",
			(_, _, 1, 0) => "│",
			(_, _, 0, 1) => "──",
			_ =>
				shots[(row - 1) / 2, (col - 1) / 2]
					? (ships[(row - 1) / 2, (col - 1) / 2] is not 0
						? hit
						: miss)
					: open,
		};
	}

	private static void GetEnterOrEscape()
	{
	GetEnterOrEscape:
		switch (Console.ReadKey(true).Key)
		{
			case ConsoleKey.Enter: break;
			case ConsoleKey.Escape: hasPressedEscape = true; break;
			default: goto GetEnterOrEscape;
		}
	}

	private static (int BufferHeight, int BufferWidth, int WindowHeight, int WindowWidth) ConsoleSize()
	{
		return (Console.BufferHeight, Console.BufferWidth, Console.WindowHeight, Console.WindowWidth);
	}
}

internal enum Ship
{
	[Tag("size", 5)] Carrier = 1,
	[Tag("size", 4)] Battleship = 2,
	[Tag("size", 3)] Cruiser = 3,
	[Tag("size", 3)] Submarine = 4,
	[Tag("size", 2)] Destroyer = 5,
}
