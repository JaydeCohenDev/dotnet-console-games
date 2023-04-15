using System;
using Towel;
using Towel.DataStructures;

namespace Battleship;

public class EnemyPlacementState
{
	private Board enemyBoard;
	
	public EnemyPlacementState(GameRenderer renderer, Board enemyBoard)
	{
		this.enemyBoard = enemyBoard;
		
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
}