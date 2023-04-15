using System;
using Towel;
using Towel.DataStructures;

public static class Program
{
	private static Board playerBoard, enemyBoard;
	private static Exception? exception;
	private static ConsoleSize consoleSize;
	private static bool isPlacing;
	private static Placement currentPlacement;
	private static bool hasPressedEscape;
	private static GridPoint gridSelection;
	private static bool isSelecting;
	private static Action? renderMessage;
	
	public static void Main(string[] args)
	{
		playerBoard = new Board(10, 10);
		enemyBoard = new Board(10, 10);
		
		try
		{
			SetupConsole();

			while (!hasPressedEscape)
			{
				// introduction screen
				ShowIntroductionScreen();
				if (hasPressedEscape) return;

				// ship placement
				EnterPlayerPlacementMode();
				if (hasPressedEscape) return;
				EnterEnemyPlacementMode();

				// shooting phase
				if (RunShootingPhase()) return;

				// game over
				ShowGameOver();
			}
		}
		catch (Exception ex)
		{
			exception = ex;
			throw;
		}
		finally
		{
			ShowShutdown();
		}

	}

	private static bool RunShootingPhase()
	{
		gridSelection = new GridPoint(playerBoard.Height / 2, playerBoard.Width / 2);
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
		while (!playerBoard.HasWon() && !enemyBoard.HasWon())
		{
			ChooseOffense();
			if (hasPressedEscape) return true;
			RandomlyChooseDefense();
			RenderMainView();
		}

		isSelecting = false;
		return false;
	}

	private static void SetupConsole()
	{
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.White;
		Console.Clear();
		consoleSize = ConsoleSize();
	}

	private static void ShowShutdown()
	{
		Console.CursorVisible = true;
		Console.ResetColor();
		Console.Clear();
		Console.WriteLine(exception?.ToString() ?? "Battleship was closed.");
	}

	private static void ShowGameOver()
	{
		Console.Clear();
		renderMessage = () =>
		{
			Console.WriteLine();
			switch ((playerBoard.HasWon(), enemyBoard.HasWon()))
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

	private static void EnterEnemyPlacementMode()
	{
		RandomizeOffenseShips();
		renderMessage = () =>
		{
			Console.WriteLine();
			Console.WriteLine("  The enemy has placed their ships.");
			Console.WriteLine();
			Console.WriteLine("  Press [enter] to continue...");
		};
		RenderMainView();
	}

	private static void EnterPlayerPlacementMode()
	{
		Console.Clear();
		PlaceDefenseShips();
	}

	private static void ShowIntroductionScreen()
	{
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
			currentPlacement = new Placement(ship, size, 0, 0, true);
			while (true)
			{
				RenderMainView();
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
						if (IsValidPlacement())
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
			var (row, column) = openLocations[Random.Shared.Next(openLocations.Count)];
			playerBoard.ShootAt(row, column);
		}
	}

	private static bool IsValidPlacement()
	{
		for (int i = 0; i < currentPlacement.Size; i++)
		{
			var row = currentPlacement.Row + (currentPlacement.Vertical ? i : 0);
			var col = currentPlacement.Column + (!currentPlacement.Vertical ? i : 0);
			if (playerBoard.GetShipAt(row, col) is not 0)
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
			for (int row = 0; row < enemyBoard.Height - size; row++)
			{
				for (int col = 0; col < enemyBoard.Width; col++)
				{
					bool vertical = true;
					bool horizontal = true;
					for (int i = 0; i < size; i++)
					{
						if (row + size > enemyBoard.Height || enemyBoard.GetShipAt(row + i, col) is not 0)
						{
							vertical = false;
						}
						if (col + size > enemyBoard.Width || enemyBoard.GetShipAt(row, col + i) is not 0)
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
				var row = Row + (Vertical ? i : 0);
				var col = Column + (!Vertical ? i : 0);
				enemyBoard.PlaceShip(ship, row, col);
			}
		}
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
		for (int row = 0; row < playerBoard.Height * 2 + 1; row++)
		{
			int boardRow = (row - 1) / 2;
			Console.Write("  ");
			for (int col = 0; col < playerBoard.Width * 2 + 1; col++)
			{
				int boardCol = (col - 1) / 2;
				bool v = boardRow + 1 < playerBoard.Height && playerBoard.GetShipAt(boardRow, boardCol) == playerBoard.GetShipAt(boardRow + 1, boardCol);
				bool h = boardCol + 1 < playerBoard.Width && playerBoard.GetShipAt(boardRow, boardCol) == playerBoard.GetShipAt(boardRow, boardCol + 1);

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
				else if (playerBoard.GetShipAt(boardRow, boardCol) is not 0 &&
					((row - 1) % 2 is 0 || ((row - 1) % 2 is 1 && v)) &&
					((col - 1) % 2 is 0 || ((col - 1) % 2 is 1 && h)))
				{
					Console.BackgroundColor = ConsoleColor.DarkGray;
				}
				Console.Write(RenderBoardTile(row, col, playerBoard.Shots, playerBoard.Ships));
				if (Console.BackgroundColor is not ConsoleColor.Black)
				{
					Console.BackgroundColor = ConsoleColor.Black;
				}
			}
			Console.Write("  ");
			for (int col = 0; col < enemyBoard.Width * 2 + 1; col++)
			{
				int boardCol = (col - 1) / 2;
				bool v = boardRow + 1 < enemyBoard.Height && enemyBoard.GetShipAt(boardRow, boardCol) == enemyBoard.GetShipAt(boardRow + 1, boardCol);
				bool h = boardCol + 1 < enemyBoard.Width && enemyBoard.GetShipAt(boardRow, boardCol) == enemyBoard.GetShipAt(boardRow, boardCol + 1);
				if (showEnemyShips &&
					enemyBoard.GetShipAt(boardRow, boardCol) is not 0 &&
					((row - 1) % 2 is 0 || ((row - 1) % 2 is 1 && v)) &&
					((col - 1) % 2 is 0 || ((col - 1) % 2 is 1 && h)))
				{
					Console.BackgroundColor = ConsoleColor.DarkGray;
				}
				else if (isSelecting && gridSelection.Row == boardRow && gridSelection.Column == boardCol &&
					(row - 1) % 2 is 0 &&
					(col - 1) % 2 is 0)
				{
					Console.BackgroundColor = ConsoleColor.DarkYellow;
				}
				Console.Write(RenderBoardTile(row, col, enemyBoard.Shots, enemyBoard.Ships));
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
		const int fullWidth = 10 * 2;
		const int fullHeight = 10 * 2;
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

	private static ConsoleSize ConsoleSize()
	{
		return new ConsoleSize
		{
			BufferHeight = Console.BufferHeight, 
			BufferWidth = Console.BufferWidth, 
			WindowHeight = Console.WindowHeight, 
			WindowWidth = Console.WindowWidth
		};
	}
}