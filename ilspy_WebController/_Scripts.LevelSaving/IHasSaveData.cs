namespace _Scripts.LevelSaving;

public interface IHasSaveData<TData>
{
	TData GetSaveData();
}
