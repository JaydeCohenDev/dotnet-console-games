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
		renderer.renderMessage = () =>
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
			
			(int Row, int Column, bool Vertical) = locations[Random.Shared.Next(0, locations.Count)];
			for (int i = 0; i < size; i++)
			{
				int row = Row + (Vertical ? i : 0);
				int col = Column + (!Vertical ? i : 0);
				_enemyBoard.PlaceShip(ship, row, col);
			}
		}
	}

	private ListArray<(int Row, int Column, bool Vertical)> FindPossiblePlacementLocations(int size)
	{
		ListArray<(int Row, int Column, bool Vertical)> locations = new();
		for (int row = 0; row < _enemyBoard.Height - size; row++)
		{
			for (int col = 0; col < _enemyBoard.Width; col++)
			{
				bool vertical = true;
				bool horizontal = true;
				for (int i = 0; i < size; i++)
				{
					if (row + size > _enemyBoard.Height || _enemyBoard.GetShipAt(row + i, col) is not 0)
					{
						vertical = false;
					}

					if (col + size > _enemyBoard.Width || _enemyBoard.GetShipAt(row, col + i) is not 0)
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

		return locations;
	}
}