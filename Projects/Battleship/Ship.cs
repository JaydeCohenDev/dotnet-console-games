using Towel;

public enum Ship
{
	[Tag("size", 5)] Carrier = 1,
	[Tag("size", 4)] Battleship = 2,
	[Tag("size", 3)] Cruiser = 3,
	[Tag("size", 3)] Submarine = 4,
	[Tag("size", 2)] Destroyer = 5,
}