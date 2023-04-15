public class Board
{
	public int Width;
	public int Height;
	public bool[,] Shots;
	public Ship[,] Ships;
	
	public Board(int width, int height)
	{
		Width = width;
		Height = height;
		Shots = new bool[width, height];
		Ships = new Ship[width, height];
	}

	public void PlaceShip(Ship ship, int row, int col)
	{
		Ships[row, col] = ship;
	}

	public bool HasShotAt(int row, int col)
	{
		return Shots[row, col];
	}

	public void ShootAt(int row, int col)
	{
		Shots[row, col] = true;
	}

	public Ship GetShipAt(int row, int col)
	{
		return Ships[row, col];
	}
	
	public bool HasWon()
	{
		for (int row = 0; row < Height; row++)
		{
			for (int col = 0; col < Width; col++)
			{
				if (Ships[row, col] is not 0 && !Shots[row, col])
				{
					return false;
				}
			}
		}
		return true;
	}
	
	public bool IsValidPlacement(Placement placement)
	{
		for (int i = 0; i < placement.Size; i++)
		{
			var row = placement.Row + (placement.Vertical ? i : 0);
			var col = placement.Column + (!placement.Vertical ? i : 0);
			if (GetShipAt(row, col) is not 0)
			{
				return false;
			}
		}
		return true;
	}
}