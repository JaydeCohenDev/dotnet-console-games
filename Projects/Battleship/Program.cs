using System;
using Battleship;

public static class Program
{
	public static Board playerBoard;
	public static Board enemyBoard;
	private static Exception? exception;
	private static ConsoleSize consoleSize;
	public static bool isPlacing;
	public static Placement currentPlacement;
	public static bool hasPressedEscape;
	public static GridPoint gridSelection;
	public static bool isSelecting;
	public static Action? renderMessage;
	
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
				new IntroductionState();
				if (hasPressedEscape) return;

				// ship placement
				new PlayerPlacementState();
				if (hasPressedEscape) return;
				new EnemyPlacementState();

				// shooting phase
				var shootingPhase = new ShootingPhaseState();
				if (shootingPhase.Run()) return;

				// game over
				new GameOverState();
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

	public static bool IsValidPlacement()
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

	public static void RenderMainView(bool showEnemyShips = false)
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

	public static void GetEnterOrEscape()
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