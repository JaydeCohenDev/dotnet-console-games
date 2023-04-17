using System;

namespace Battleship;

public struct GridPoint
{
	public int Row;
	public int Column;

	public GridPoint(int row, int column)
	{
		Row = row;
		Column = column;
	}

	private bool Equals(GridPoint other)
	{
		return Row == other.Row && Column == other.Column;
	}

	public override bool Equals(object? obj)
	{
		return obj is GridPoint other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Row, Column);
	}

	public static bool operator ==(GridPoint left, GridPoint right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(GridPoint left, GridPoint right)
	{
		return !(left == right);
	}
}