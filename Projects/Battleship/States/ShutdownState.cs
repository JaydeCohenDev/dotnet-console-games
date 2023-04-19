using System;

namespace Battleship.States;

public class ShutdownState : IGameState
{
	private readonly Exception? _ex;

	public ShutdownState(Exception? ex)
	{
		_ex = ex;
	}

	public bool Enter()
	{
		Console.CursorVisible = true;
		Console.ResetColor();
		Console.Clear();
		Console.WriteLine(_ex?.ToString() ?? "Battleship was closed.");
		return false;
	}

	public void Render(GameRenderer renderer)
	{
		throw new NotImplementedException();
	}
}