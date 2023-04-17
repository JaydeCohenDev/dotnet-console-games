using System;
using System.Collections.Generic;

namespace Battleship;

public class InputHandler
{
	public bool HasPressedEscape { get; set; }
	private readonly Dictionary<object, Dictionary<ConsoleKey, Action>> _boundActions;

	public InputHandler()
	{
		_boundActions = new Dictionary<object, Dictionary<ConsoleKey, Action>>();
	}

	public void StartListeningForInput(object context)
	{
		while(true) {
			var key = Console.ReadKey(true).Key;
			var action = GetBoundActionForContext(context, key);
			if (action is null) continue;
			
			action.Invoke();
			return;
		}
	}
	
	public void AddActionBinding(object context, ConsoleKey key, Action action)
	{
		if (!_boundActions.ContainsKey(context))
			_boundActions.Add(context, new Dictionary<ConsoleKey, Action>());

		var actionSet = _boundActions[context];
		if (!actionSet.TryAdd(key, action))
			actionSet[key] = action;
	}

	public void RemoveActionBinding(object context, ConsoleKey key)
	{
		if (!_boundActions.ContainsKey(context)) return;
		_boundActions[context].Remove(key);
	}

	public void RemoveAllActionBindings(object context)
	{
		_boundActions.Remove(context);
	}

	private Action? GetBoundActionForContext(object context, ConsoleKey key)
	{
		if (!_boundActions.ContainsKey(context)) return null;
		return _boundActions[context].ContainsKey(key) ? _boundActions[context][key] : null;
	}
	
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