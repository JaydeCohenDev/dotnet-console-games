using System;

namespace Battleship;

public class ShutdownState
{
	public ShutdownState()
	{
		Console.CursorVisible = true;
		Console.ResetColor();
		Console.Clear();
		Console.WriteLine(Program.exception?.ToString() ?? "Battleship was closed.");
	}
}