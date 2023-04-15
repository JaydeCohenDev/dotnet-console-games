using System;

namespace Battleship;

public class InputHandler
{
	public bool HasPressedEscape { get; set; }
	
	public void GetEnterOrEscape()
	{
		while (true)
		{
			switch (Console.ReadKey(true).Key)
			{
				case ConsoleKey.Enter: return;
				case ConsoleKey.Escape: HasPressedEscape = true; return;
			}
		}
	}
}