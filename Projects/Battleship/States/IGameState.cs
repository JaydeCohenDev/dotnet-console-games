namespace Battleship.States;

public interface IGameState
{
	void Render(GameRenderer renderer);
	bool Enter();
}