using System;

namespace Battleship;

public class InputHandler
{
	public bool hasPressedEscape { get; set; }
	
	public void GetEnterOrEscape()
	{
		GetEnterOrEscape:
		switch (Console.ReadKey(true).Key)
		{
			case ConsoleKey.Enter: break;
			case ConsoleKey.Escape: hasPressedEscape = true; break;
			default: goto GetEnterOrEscape;
		}
	}
}