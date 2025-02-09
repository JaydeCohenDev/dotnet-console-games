﻿using System;
using Battleship.States;

namespace Battleship;

public class GameRenderer
{
	public static GameRenderer Global;
	
	public Action? RenderMessage;
	private ConsoleSize _consoleSize;

	private readonly Board _playerBoard;
	private readonly Board _enemyBoard;
	private readonly PlayerPlacementState _placementState;
	private readonly ShootingPhaseState _shootingPhaseState;

	public GameRenderer(Board playerBoard, Board enemyBoard, PlayerPlacementState placementState, ShootingPhaseState shootingPhaseState)
	{
		_playerBoard = playerBoard;
		_enemyBoard = enemyBoard;
		_placementState = placementState;
		_shootingPhaseState = shootingPhaseState;

		Global = this;
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
		SetupMainViewPass();
		for (int row = 0; row < _playerBoard.Height * 2 + 1; row++)
		{
			int boardRow = (row - 1) / 2;
			Console.Write("  ");
			RenderPlayerBoard(boardRow, row);
			Console.Write("  ");
			RenderEnemyBoard(showEnemyShips, boardRow, row);
			Console.WriteLine();
		}
		RenderMessage?.Invoke();
	}

	private void RenderPlayerBoard(int boardRow, int row)
	{
		for (int col = 0; col < _playerBoard.Width * 2 + 1; col++)
		{
			int boardCol = (col - 1) / 2;
			bool vertical = IsVertical(boardRow, boardCol, _playerBoard);
			bool horizontal = IsHorizontal(boardRow, boardCol, _playerBoard);

			SetPlayerTileBackgroundColor(boardCol, boardRow, col, row, vertical, horizontal);

			Console.Write(RenderBoardTile(row, col, _playerBoard));
			ResetBackgroundColor();
		}
	}
	
	private void RenderEnemyBoard(bool showEnemyShips, int boardRow, int row)
	{
		for (int col = 0; col < _enemyBoard.Width * 2 + 1; col++)
		{
			int boardCol = (col - 1) / 2;
			bool vertical = IsVertical(boardRow, boardCol, _enemyBoard);
			bool horizontal = IsHorizontal(boardRow, boardCol, _enemyBoard);
			
			SetEnemyTileBackgroundColor(showEnemyShips, boardRow, row, boardCol, vertical, col, horizontal);
			
			Console.Write(RenderBoardTile(row, col, _enemyBoard));
			ResetBackgroundColor();
		}
	}

	private void ResetBackgroundColor()
	{
		if (Console.BackgroundColor is not ConsoleColor.Black)
			Console.BackgroundColor = ConsoleColor.Black;
	}

	private bool IsVertical(int boardRow, int boardCol, Board board)
	{
		return boardRow + 1 < board.Height && board.GetShipAt(boardRow, boardCol) ==
			board.GetShipAt(boardRow + 1, boardCol);
	}

	private bool IsHorizontal(int boardRow, int boardCol, Board board)
	{
		return boardCol + 1 < board.Width && board.GetShipAt(boardRow, boardCol) ==
			board.GetShipAt(boardRow, boardCol + 1);
	}

	private void SetEnemyTileBackgroundColor(bool showEnemyShips, int boardRow, int row, int boardCol, bool vertical, int col,
		bool horizontal)
	{
		if (CellHasVisibleEnemyShip(showEnemyShips, boardRow, row, boardCol, vertical, col, horizontal))
		{
			Console.BackgroundColor = ConsoleColor.DarkGray;
		}
		else if (IsCellAimedAt(boardRow, row, boardCol, col))
		{
			Console.BackgroundColor = ConsoleColor.DarkYellow;
		}
	}

	private void SetPlayerTileBackgroundColor(int boardCol, int boardRow, int col, int row, bool vertical, bool horizontal)
	{
		if (IsPlacingVerticalShipInCell(boardCol, boardRow, col, row) || IsPlacingHorizontalShipInCell(boardCol, boardRow, col, row))
		{
			Console.BackgroundColor = _playerBoard.IsValidPlacement(_placementState.CurrentPlacement)
				? ConsoleColor.DarkGreen
				: ConsoleColor.DarkRed;
		}
		else if (HasPlacedShipInCell(boardCol, boardRow, col, row, vertical, horizontal))
		{
			Console.BackgroundColor = ConsoleColor.DarkGray;
		}
	}

	private bool HasPlacedShipInCell(int boardCol, int boardRow, int col, int row, bool vertical, bool horizontal)
	{
		return _playerBoard.HasShipAt(boardRow, boardCol) &&
		       ((row - 1) % 2 is 0 || ((row - 1) % 2 is 1 && vertical)) &&
		       ((col - 1) % 2 is 0 || ((col - 1) % 2 is 1 && horizontal));
	}

	private bool IsPlacingHorizontalShipInCell(int boardCol, int boardRow, int col, int row)
	{
		return _placementState is { IsPlacing: true, CurrentPlacement.Vertical: false } &&
		       boardRow == _placementState.CurrentPlacement.Row &&
		       boardCol >= _placementState.CurrentPlacement.Column &&
		       boardCol < _placementState.CurrentPlacement.Column + _placementState.CurrentPlacement.Size &&
		       (row - 1) % 2 is 0 &&
		       !(boardCol == _placementState.CurrentPlacement.Column + _placementState.CurrentPlacement.Size -
			       1 && (col - 1) % 2 is 1) &&
		       col is not 0;
	}

	private bool IsPlacingVerticalShipInCell(int boardCol, int boardRow, int col, int row)
	{
		return _placementState is { IsPlacing: true, CurrentPlacement.Vertical: true } &&
		       boardCol == _placementState.CurrentPlacement.Column &&
		       boardRow >= _placementState.CurrentPlacement.Row &&
		       boardRow < _placementState.CurrentPlacement.Row + _placementState.CurrentPlacement.Size &&
		       (col - 1) % 2 is 0 &&
		       !(boardRow == _placementState.CurrentPlacement.Row + _placementState.CurrentPlacement.Size - 1 &&
		         (row - 1) % 2 is 1) &&
		       row is not 0;
	}

	private void SetupMainViewPass()
	{
		Console.CursorVisible = false;
		ValidateConsoleSize();

		Console.SetCursorPosition(0, 0);
		Console.WriteLine();
		Console.WriteLine("  Battleship");
		Console.WriteLine();
	}
	
	private bool CellHasVisibleEnemyShip(bool showEnemyShips, int boardRow, int row, int boardCol, bool vertical, int col, bool horizontal)
	{
		return showEnemyShips &&
		       _enemyBoard.HasShipAt(boardRow, boardCol) &&
		       ((row - 1) % 2 is 0 || ((row - 1) % 2 is 1 && vertical)) &&
		       ((col - 1) % 2 is 0 || ((col - 1) % 2 is 1 && horizontal));
	}

	private bool IsCellAimedAt(int boardRow, int row, int boardCol, int col)
	{
		return _shootingPhaseState.IsSelecting && _shootingPhaseState.GridSelection.Row == boardRow &&
		       _shootingPhaseState.GridSelection.Column == boardCol &&
		       (row - 1) % 2 is 0 &&
		       (col - 1) % 2 is 0;
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

		if (_consoleSize == GetConsoleSize()) return;
		Console.Clear();
		_consoleSize = GetConsoleSize();
	}

	private string RenderBoardTile(int row, int col, Board board)
	{
		const string hit = "##";
		const string miss = "XX";
		const string open = "  ";
		const int fullWidth = 10 * 2;
		const int fullHeight = 10 * 2;
		return (row, col, row % 2, col % 2) switch
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
				board.Shots[(row - 1) / 2, (col - 1) / 2]
					? (board.Ships[(row - 1) / 2, (col - 1) / 2] is not 0
						? hit
						: miss)
					: open,
		};
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
}