using System;
using Towel;
using Towel.DataStructures;

namespace Battleship.States;

public class EnemyPlacementState
{
	private readonly Board _enemyBoard;
	
	public EnemyPlacementState(GameRenderer renderer, Board enemyBoard)
	{
		_enemyBoard = enemyBoard;
		
		RandomizeOffenseShips();
		renderer.RenderMessage = () =>
		{
			Console.WriteLine();
			Console.WriteLine("  The enemy has placed their ships.");
			Console.WriteLine();
			Console.WriteLine("  Press [enter] to continue...");
		};
		renderer.RenderMainView();
	}
	
	private void RandomizeOffenseShips()
	{
		foreach (Ship ship in Enum.GetValues<Ship>())
		{
			int size = (int)ship.GetTag("size").Value!;
			var locations = FindPossiblePlacementLocations(size);
			
			AiPlacement placement = locations[Random.Shared.Next(0, locations.Count)];
			PlaceShip(size, placement, ship);
		}
	}

	private void PlaceShip(int size, AiPlacement placement, Ship ship)
	{
		for (int i = 0; i < size; i++)
		{
			int row = placement.Row + (placement.Vertical ? i : 0);
			int col = placement.Column + (!placement.Vertical ? i : 0);
			_enemyBoard.PlaceShip(ship, row, col);
		}
	}

	private ListArray<AiPlacement> FindPossiblePlacementLocations(int size)
	{
		ListArray<AiPlacement> locations = new();
		for (int row = 0; row < _enemyBoard.Height - size; row++)
		{
			for (int col = 0; col < _enemyBoard.Width; col++)
			{
				bool vertical = true;
				bool horizontal = true;
				for (int i = 0; i < size; i++)
				{
					if (row + size > _enemyBoard.Height || _enemyBoard.GetShipAt(row + i, col) is not 0)
						vertical = false;

					if (col + size > _enemyBoard.Width || _enemyBoard.GetShipAt(row, col + i) is not 0)
						horizontal = false;
				}

				if (vertical) locations.Add(new AiPlacement(row, col, true));
				if (horizontal) locations.Add(new AiPlacement(row, col, false));
			}
		}

		return locations;
	}

	private struct AiPlacement
	{
		public readonly int Row;
		public readonly int Column;
		public readonly bool Vertical;

		public AiPlacement(int row, int column, bool vertical)
		{
			Row = row;
			Column = column;
			Vertical = vertical;
		}
	}
}