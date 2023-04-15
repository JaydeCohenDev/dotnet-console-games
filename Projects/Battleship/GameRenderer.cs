using System;

namespace Battleship;

public class GameRenderer
{
	public Action? renderMessage;
	public Exception? exception;
	private ConsoleSize consoleSize;
	
	private ConsoleSize ConsoleSize()
	{
		return new ConsoleSize
		{
			BufferHeight = Console.BufferHeight, 
			BufferWidth = Console.BufferWidth, 
			WindowHeight = Console.WindowHeight, 
			WindowWidth = Console.WindowWidth
		};
	}

	public void SetupConsole()
	{
		Console.BackgroundColor = ConsoleColor.Black;
		Console.ForegroundColor = ConsoleColor.White;
		Console.Clear();
		consoleSize = ConsoleSize();
	}
	
	public void RenderMainView(bool showEnemyShips = false)
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
		for (int row = 0; row < Program.playerBoard.Height * 2 + 1; row++)
		{
			int boardRow = (row - 1) / 2;
			Console.Write("  ");
			for (int col = 0; col < Program.playerBoard.Width * 2 + 1; col++)
			{
				int boardCol = (col - 1) / 2;
				bool v = boardRow + 1 < Program.playerBoard.Height && Program.playerBoard.GetShipAt(boardRow, boardCol) == Program.playerBoard.GetShipAt(boardRow + 1, boardCol);
				bool h = boardCol + 1 < Program.playerBoard.Width && Program.playerBoard.GetShipAt(boardRow, boardCol) == Program.playerBoard.GetShipAt(boardRow, boardCol + 1);

				if (PlayerPlacementState.isPlacing &&
				    PlayerPlacementState.currentPlacement.Vertical &&
				    boardCol == PlayerPlacementState.currentPlacement.Column &&
				    boardRow >= PlayerPlacementState.currentPlacement.Row &&
				    boardRow < PlayerPlacementState.currentPlacement.Row + PlayerPlacementState.currentPlacement.Size &&
				    (col - 1) % 2 is 0 &&
				    !(boardRow == PlayerPlacementState.currentPlacement.Row + PlayerPlacementState.currentPlacement.Size - 1 && (row - 1) % 2 is 1) &&
				    row is not 0)
				{
					Console.BackgroundColor = Program.playerBoard.IsValidPlacement(PlayerPlacementState.currentPlacement) ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
				}
				else if (PlayerPlacementState.isPlacing &&
				         !PlayerPlacementState.currentPlacement.Vertical &&
				         boardRow == PlayerPlacementState.currentPlacement.Row &&
				         boardCol >= PlayerPlacementState.currentPlacement.Column &&
				         boardCol < PlayerPlacementState.currentPlacement.Column + PlayerPlacementState.currentPlacement.Size &&
				         (row - 1) % 2 is 0 &&
				         !(boardCol == PlayerPlacementState.currentPlacement.Column + PlayerPlacementState.currentPlacement.Size - 1 && (col - 1) % 2 is 1) &&
				         col is not 0)
				{
					Console.BackgroundColor = Program.playerBoard.IsValidPlacement(PlayerPlacementState.currentPlacement) ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
				}
				else if (Program.playerBoard.GetShipAt(boardRow, boardCol) is not 0 &&
				         ((row - 1) % 2 is 0 || ((row - 1) % 2 is 1 && v)) &&
				         ((col - 1) % 2 is 0 || ((col - 1) % 2 is 1 && h)))
				{
					Console.BackgroundColor = ConsoleColor.DarkGray;
				}
				Console.Write(RenderBoardTile(row, col, Program.playerBoard.Shots, Program.playerBoard.Ships));
				if (Console.BackgroundColor is not ConsoleColor.Black)
				{
					Console.BackgroundColor = ConsoleColor.Black;
				}
			}
			Console.Write("  ");
			for (int col = 0; col < Program.enemyBoard.Width * 2 + 1; col++)
			{
				int boardCol = (col - 1) / 2;
				bool v = boardRow + 1 < Program.enemyBoard.Height && Program.enemyBoard.GetShipAt(boardRow, boardCol) == Program.enemyBoard.GetShipAt(boardRow + 1, boardCol);
				bool h = boardCol + 1 < Program.enemyBoard.Width && Program.enemyBoard.GetShipAt(boardRow, boardCol) == Program.enemyBoard.GetShipAt(boardRow, boardCol + 1);
				if (showEnemyShips &&
				    Program.enemyBoard.GetShipAt(boardRow, boardCol) is not 0 &&
					((row - 1) % 2 is 0 || ((row - 1) % 2 is 1 && v)) &&
					((col - 1) % 2 is 0 || ((col - 1) % 2 is 1 && h)))
				{
					Console.BackgroundColor = ConsoleColor.DarkGray;
				}
				else if (ShootingPhaseState.isSelecting && ShootingPhaseState.gridSelection.Row == boardRow && ShootingPhaseState.gridSelection.Column == boardCol &&
				         (row - 1) % 2 is 0 &&
				         (col - 1) % 2 is 0)
				{
					Console.BackgroundColor = ConsoleColor.DarkYellow;
				}
				Console.Write(RenderBoardTile(row, col, Program.enemyBoard.Shots, Program.enemyBoard.Ships));
				if (Console.BackgroundColor is not ConsoleColor.Black)
				{
					Console.BackgroundColor = ConsoleColor.Black;
				}
			}
			Console.WriteLine();
		}
		renderMessage?.Invoke();

		
	}
	
	private string RenderBoardTile(int row, int col, bool[,] shots, Ship[,] ships)
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
}