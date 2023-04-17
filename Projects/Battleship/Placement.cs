namespace Battleship;

public struct Placement
{
	public Ship Ship;
	public readonly int Size;
	public int Row;
	public int Column;
	public bool Vertical;

	public Placement(Ship ship, int size, int row, int column, bool vertical)
	{
		Ship = ship;
		Size = size;
		Row = row;
		Column = column;
		Vertical = vertical;
	}
}