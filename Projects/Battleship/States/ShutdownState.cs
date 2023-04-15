using System;

namespace Battleship;

public class ShutdownState
{
	public ShutdownState()
	{
		Console.CursorVisible = true;
		Console.ResetColor();
		Console.Clear();
		Console.WriteLine(Program.renderer.exception?.ToString() ?? "Battleship was closed.");
	}
}