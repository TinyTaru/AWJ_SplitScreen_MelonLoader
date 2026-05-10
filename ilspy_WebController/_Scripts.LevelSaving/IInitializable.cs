namespace _Scripts.LevelSaving;

public interface IInitializable<TData>
{
	void Initialize(TData data);
}
