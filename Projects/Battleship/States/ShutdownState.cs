using System;

namespace Battleship.States;

public class ShutdownState
{
	public ShutdownState(Exception? ex)
	{
		Console.CursorVisible = true;
		Console.ResetColor();
		Console.Clear();
		Console.WriteLine(ex?.ToString() ?? "Battleship was closed.");
	}
}