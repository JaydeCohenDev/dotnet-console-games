using System;

namespace Battleship;

public class ShutdownState
{
	public ShutdownState(GameRenderer renderer)
	{
		Console.CursorVisible = true;
		Console.ResetColor();
		Console.Clear();
		Console.WriteLine(renderer.exception?.ToString() ?? "Battleship was closed.");
	}
}