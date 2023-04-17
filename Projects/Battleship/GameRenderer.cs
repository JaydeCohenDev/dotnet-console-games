using System;
using Battleship.States;

namespace Battleship;

public class GameRenderer
{
	public Action? RenderMessage;
	private ConsoleSize _consoleSize;

	private readonly Board _playerBoard;
	private readonly Board _enemyBoard;

	public GameRenderer(Board playerBoard, Board enemyBoard)
	{
		_playerBoard = playerBoard;
		_enemyBoard = enemyBoard;
	}
	
	private ConsoleSize GetConsoleSize()
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
		_consoleSize = GetConsoleSize();
	}
	
	public void RenderMainView(bool showEnemyShips = false)
	{
		Console.CursorVisible = false;
		ValidateConsoleSize();

		Console.SetCursorPosition(0, 0);
		Console.WriteLine();
		Console.WriteLine("  Battleship");
		Console.WriteLine();
		for (int row = 0; row < _playerBoard.Height * 2 + 1; row++)
		{
			int boardRow = (row - 1) / 2;
			Console.Write("  ");
			for (int col = 0; col < _playerBoard.Width * 2 + 1; col++)
			{
				int boardCol = (col - 1) / 2;
				bool v = boardRow + 1 < _playerBoard.Height && _playerBoard.GetShipAt(boardRow, boardCol) == _playerBoard.GetShipAt(boardRow + 1, boardCol);
				bool h = boardCol + 1 < _playerBoard.Width && _playerBoard.GetShipAt(boardRow, boardCol) == _playerBoard.GetShipAt(boardRow, boardCol + 1);

				if (PlayerPlacementState.isPlacing &&
				    PlayerPlacementState.currentPlacement.Vertical &&
				    boardCol == PlayerPlacementState.currentPlacement.Column &&
				    boardRow >= PlayerPlacementState.currentPlacement.Row &&
				    boardRow < PlayerPlacementState.currentPlacement.Row + PlayerPlacementState.currentPlacement.Size &&
				    (col - 1) % 2 is 0 &&
				    !(boardRow == PlayerPlacementState.currentPlacement.Row + PlayerPlacementState.currentPlacement.Size - 1 && (row - 1) % 2 is 1) &&
				    row is not 0)
				{
					Console.BackgroundColor = _playerBoard.IsValidPlacement(PlayerPlacementState.currentPlacement) ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
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
					Console.BackgroundColor = _playerBoard.IsValidPlacement(PlayerPlacementState.currentPlacement) ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
				}
				else if (_playerBoard.GetShipAt(boardRow, boardCol) is not 0 &&
				         ((row - 1) % 2 is 0 || ((row - 1) % 2 is 1 && v)) &&
				         ((col - 1) % 2 is 0 || ((col - 1) % 2 is 1 && h)))
				{
					Console.BackgroundColor = ConsoleColor.DarkGray;
				}
				
				
				Console.Write(RenderBoardTile(row, col, _playerBoard.Shots, _playerBoard.Ships));
				if (Console.BackgroundColor is not ConsoleColor.Black)
				{
					Console.BackgroundColor = ConsoleColor.Black;
				}
				
				
			}
			Console.Write("  ");
			for (int col = 0; col < _enemyBoard.Width * 2 + 1; col++)
			{
				int boardCol = (col - 1) / 2;
				bool v = boardRow + 1 < _enemyBoard.Height && _enemyBoard.GetShipAt(boardRow, boardCol) == _enemyBoard.GetShipAt(boardRow + 1, boardCol);
				bool h = boardCol + 1 < _enemyBoard.Width && _enemyBoard.GetShipAt(boardRow, boardCol) == _enemyBoard.GetShipAt(boardRow, boardCol + 1);
				if (showEnemyShips &&
				    _enemyBoard.GetShipAt(boardRow, boardCol) is not 0 &&
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
				Console.Write(RenderBoardTile(row, col, _enemyBoard.Shots, _enemyBoard.Ships));
				if (Console.BackgroundColor is not ConsoleColor.Black)
				{
					Console.BackgroundColor = ConsoleColor.Black;
				}
			}
			Console.WriteLine();
		}
		RenderMessage?.Invoke();

		
	}

	private void ValidateConsoleSize()
	{
		if (OperatingSystem.IsWindows() && Console.BufferHeight != Console.WindowHeight)
		{
			Console.BufferHeight = Console.WindowHeight;
		}

		if (OperatingSystem.IsWindows() && Console.BufferWidth != Console.WindowWidth)
		{
			Console.BufferWidth = Console.WindowWidth;
		}

		if (_consoleSize != GetConsoleSize())
		{
			Console.Clear();
			_consoleSize = GetConsoleSize();
		}
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