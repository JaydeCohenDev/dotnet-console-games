using System;

public struct ConsoleSize
{
	public int BufferHeight;
	public int BufferWidth;
	public int WindowHeight;
	public int WindowWidth;
	
	public bool Equals(ConsoleSize other)
	{
		return BufferHeight == other.BufferHeight && BufferWidth == other.BufferWidth && WindowHeight == other.WindowHeight && WindowWidth == other.WindowWidth;
	}

	public override bool Equals(object? obj)
	{
		return obj is ConsoleSize other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(BufferHeight, BufferWidth, WindowHeight, WindowWidth);
	}

	public static bool operator ==(ConsoleSize left, ConsoleSize right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(ConsoleSize left, ConsoleSize right)
	{
		return !(left == right);
	}
}