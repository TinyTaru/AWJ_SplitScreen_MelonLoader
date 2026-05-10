namespace _Scripts.LevelSaving;

public struct MeltableObjectSaveData : IHasId
{
	public string id;

	public string name;

	public float meltAmount;

	public string Id => id;
}
